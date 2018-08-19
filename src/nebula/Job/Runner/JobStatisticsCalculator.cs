using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ComposerCore.Attributes;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Job.Runner
{
    [Contract]
    [Component]
    [ComponentCache(null)]
    internal class JobStatisticsCalculator
    {
        private const int SecondsBetweenFlushes = 4;
        
        private readonly IJobStore _jobStore;

        private string _tenantId;
        private string _jobId;
        private bool _initialized;
        private long _lastFlushTicks;

        private long _lastIterationStartedTicks;
        private long _lastDequeueAttemptTicks;
        private long _lastProcessStartedTicks;
        private long _lastProcessFinishedTicks;
        private long _lastHealthCheckTicks;
        private long _lastFailTicks;
        
        private long _unflushedItemsProcessed;
        private long _unflushedProcessingTimeTakenMillis;
        private long _unflushedItemsFailed;
        private long _unflushedItemsRequeued;
        private long _unflushedItemsGeneratedForTargetQueue;
        private ConcurrentQueue<string> _unflushedProcessingErrorMessages;

        private long LastFlushTicks => Interlocked.Read(ref _lastFlushTicks);
        private long TicksSinceLastFlush => DateTime.UtcNow.Ticks - LastFlushTicks;

        private long LastIterationStartedTicks => Interlocked.Read(ref _lastIterationStartedTicks);
        public long TicksSinceLastIteration => DateTime.UtcNow.Ticks - LastIterationStartedTicks;
        
        private long LastDequeueAttemptTicks => Interlocked.Read(ref _lastDequeueAttemptTicks);
        private long LastProcessStartedTicks => Interlocked.Read(ref _lastProcessStartedTicks);
        private long LastProcessFinishedTicks => Interlocked.Read(ref _lastProcessFinishedTicks);
        private long LastHealthCheckTicks => Interlocked.Read(ref _lastHealthCheckTicks);
        private long LastFailTicks => Interlocked.Read(ref _lastFailTicks);

        public long TicksBetweenLastProcessStartAndFinish => LastProcessFinishedTicks - LastProcessStartedTicks;

        
        [CompositionConstructor]
        public JobStatisticsCalculator(IJobStore jobStore)
        {
            _jobStore = jobStore;
        }

        public void Initialize(JobData jobData)
        {
            if (_initialized)
                throw new InvalidOperationException("Already initialized");

            _initialized = true;
            _tenantId = jobData.TenantId;
            _jobId = jobData.JobId;

            _lastIterationStartedTicks = DateTime.UtcNow.Ticks;
            _lastDequeueAttemptTicks = DateTime.UtcNow.Ticks;
            _lastProcessStartedTicks = DateTime.UtcNow.Ticks;
            _lastProcessFinishedTicks = DateTime.UtcNow.Ticks;
            _lastHealthCheckTicks = DateTime.UtcNow.Ticks;
            _lastFailTicks = 0;
        
            _unflushedItemsProcessed = 0;
            _unflushedProcessingTimeTakenMillis = 0;
            _unflushedItemsFailed = 0;
            _unflushedItemsRequeued = 0;
            _unflushedItemsGeneratedForTargetQueue = 0;
            _unflushedProcessingErrorMessages = new ConcurrentQueue<string>();
        }

        public void Flush()
        {
            FlushInternal();
        }

        public void ReportStartingHealthCheck()
        {
            Interlocked.Exchange(ref _lastHealthCheckTicks, DateTime.UtcNow.Ticks);
            FlushIfNecessary();
        }

        public void ReportIterationStarted()
        {
            Interlocked.Exchange(ref _lastIterationStartedTicks, DateTime.UtcNow.Ticks);
            FlushIfNecessary();
        }

        public void ReportDequeueAttempt()
        {
            Interlocked.Exchange(ref _lastDequeueAttemptTicks, DateTime.UtcNow.Ticks);
            FlushIfNecessary();
        }
        
        public void ReportProcessStarted()
        {
            Interlocked.Exchange(ref _lastProcessStartedTicks, DateTime.UtcNow.Ticks);
            FlushIfNecessary();
        }

        public void ReportProcessFinished(int itemsProcessed, TimeSpan elapsedTime, JobProcessingResult processingResult)
        {
            Interlocked.Exchange(ref _lastProcessFinishedTicks, DateTime.UtcNow.Ticks);

            Interlocked.Add(ref _unflushedItemsProcessed, itemsProcessed);
            Interlocked.Add(ref _unflushedProcessingTimeTakenMillis, (long) elapsedTime.TotalMilliseconds);
            Interlocked.Add(ref _unflushedItemsFailed, processingResult.ItemsFailed);
            Interlocked.Add(ref _unflushedItemsRequeued, processingResult.ItemsRequeued);
            Interlocked.Add(ref _unflushedItemsGeneratedForTargetQueue, processingResult.ItemsGeneratedForTargetQueue);
            
            if (processingResult.ItemsFailed > 0)
                Interlocked.Exchange(ref _lastFailTicks, DateTime.UtcNow.Ticks);
            
            if (processingResult.FailureMessages != null)
                foreach (var error in processingResult.FailureMessages)
                    _unflushedProcessingErrorMessages.Enqueue(error);
            
            FlushIfNecessary();
        }

        public void ReportPausedIterationFinished()
        {
            Interlocked.Exchange(ref _lastProcessFinishedTicks, DateTime.UtcNow.Ticks);
            FlushIfNecessary();
        }
        
        public void ReportHealthCheckException(Exception exception)
        {
            ReportException(nameof(ReportHealthCheckException), exception);
        }

        public void ReportFatalException(Exception exception)
        {
            ReportException(nameof(ReportFatalException), exception);
        }

        public void ReportIterationException(Exception exception)
        {
            ReportException(nameof(ReportIterationException), exception);
        }

        public void ReportProcessorInvocationException(Exception exception)
        {
            Interlocked.Exchange(ref _lastProcessFinishedTicks, DateTime.UtcNow.Ticks);
            ReportException(nameof(ReportProcessorInvocationException), exception);
        }

        #region Private helper methods

        private void ReportException(string source, Exception exception)
        {
            var message = $"{source}: {exception.Message}";
            
            while (exception?.InnerException != null)
            {
                message += Environment.NewLine + exception.InnerException.Message;
                exception = exception.InnerException;
            }
            
            _jobStore.AddException(_tenantId, _jobId, new JobStatusErrorData
                {
                    ErrorMessage = message,
                    Timestamp = DateTime.UtcNow.Ticks,
                    StackTrace = exception.StackTrace
                })
                .GetAwaiter()
                .GetResult();
        }

        private void FlushInternal()
        {
            Interlocked.Exchange(ref _lastFlushTicks, DateTime.UtcNow.Ticks);
            
            var lastFailTicks = LastFailTicks;
            var lastFailTime = lastFailTicks == 0 ? (DateTime?)null : new DateTime(lastFailTicks);

            var change = new JobStatusUpdateData
            {
                LastIterationStartTime = new DateTime(LastIterationStartedTicks, DateTimeKind.Utc),
                LastDequeueAttemptTime = new DateTime(LastDequeueAttemptTicks, DateTimeKind.Utc),
                LastProcessStartTime = new DateTime(LastProcessStartedTicks, DateTimeKind.Utc),
                LastProcessFinishTime = new DateTime(LastProcessFinishedTicks, DateTimeKind.Utc),
                LastHealthCheckTime = new DateTime(LastHealthCheckTicks, DateTimeKind.Utc),

                ItemsProcessedDelta = Interlocked.Exchange(ref _unflushedItemsProcessed, 0L),
                ProcessingTimeTakenMillisDelta = Interlocked.Exchange(ref _unflushedProcessingTimeTakenMillis, 0L),
                ItemsRequeuedDelta = Interlocked.Exchange(ref _unflushedItemsRequeued, 0L),
                ItemsGeneratedForTargetQueueDelta =
                    Interlocked.Exchange(ref _unflushedItemsGeneratedForTargetQueue, 0L),

                ItemsFailedDelta = Interlocked.Exchange(ref _unflushedItemsFailed, 0L),
                LastFailTime = lastFailTime
            };

            if (!_unflushedProcessingErrorMessages.IsEmpty)
            {
                var faults = new List<JobStatusErrorData>();
                while (_unflushedProcessingErrorMessages.TryDequeue(out var message))
                    faults.Add(new JobStatusErrorData { ErrorMessage = message, Timestamp = DateTime.UtcNow.Ticks});

                if (faults.Count > 0)
                    change.LastFailures = faults.ToArray();
            }
            
            _jobStore.UpdateStatus(_tenantId, _jobId, change).GetAwaiter().GetResult();
        }

        private void FlushIfNecessary()
        {
            if (TicksSinceLastFlush > SecondsBetweenFlushes * TimeSpan.TicksPerSecond)
                FlushInternal();
        }

        #endregion
    }
}