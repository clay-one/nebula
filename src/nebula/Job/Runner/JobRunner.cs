using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Attributes;
using hydrogen.General.Collections;
using log4net;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Job.Runner
{
    public class JobRunner
    {
        protected static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    [Component]
    [ComponentCache(null)]
    public class JobRunner<TJobStep> : JobRunner, IJobRunner<TJobStep> where TJobStep : IJobStep
    {
        private const int WaitMillisWhenTargetQueueIsFull = 1000;
        private const int WaitMillisWhenThereIsNoMoreWork = 2000;
        private const int WaitMillisWhenJobIsPaused = 3000;
        private const int WaitMillisWhenJobIsExpired = 5000;
        private const int WaitMillisForTaskSemaphore = 5000;

        private const int MinimumWaitMillis = 10;
        private const int MaximumWaitMillis = 2000;
        private const int MinimumWaitMillisBeforeConsideredAsStalled = 10000;

        private const int DefaultIdleSecondsToCompletion = 30;

        private readonly IComposer _composer;
        private readonly IJobProcessor<TJobStep> _processor;
        private readonly IJobStore _jobStore;
        private readonly IJobRunnerManager _jobRunnerManager;
        private readonly JobStatisticsCalculator _statistics;
        private readonly IJobNotification _jobNotification;

        private string _tenantId;
        private string _jobId;
        private JobData _jobData;
        private ThrottleCalculator _throttleCalculator;

        private bool _initialized;
        private volatile bool _started;
        private volatile bool _stopping;
        private volatile bool _terminated;
        private SemaphoreSlim _taskQueueSemaphore;

        private volatile JobStatusData _lastStatus;

        [CompositionConstructor]
        public JobRunner(IComposer composer, IJobProcessor<TJobStep> processor, IJobStore jobStore,
            IJobRunnerManager jobRunnerManager, JobStatisticsCalculator statistics, IJobNotification jobNotification)
        {
            _composer = composer;
            _processor = processor;
            _jobStore = jobStore;
            _jobRunnerManager = jobRunnerManager;
            _statistics = statistics;
            _jobNotification = jobNotification;
        }

        public string TenantId => _tenantId;
        public string JobId => _jobId;

        public bool IsProcessRunning => _started && !_terminated;

        public bool IsProcessTerminated => _terminated;

        public void Initialize(JobData jobData)
        {
            if (_initialized)
            {
                // Probably called twice because of race condition in JobRunnerManager. Just log and ignore.
                Log.Info($"Job runner {_jobId} - Initialize is called for the second time.");
                return;
            }

            _initialized = true;

            _tenantId = jobData.TenantId;
            _jobId = jobData.JobId;
            _jobData = jobData;
            _lastStatus = jobData.Status;
            _processor.Initialize(_jobData);
            
            Log.Info($"Job runner {_jobId} - Performing runner initialization");

            _statistics.Initialize(jobData);

            if (jobData.Configuration.ThrottledItemsPerSecond.HasValue)
                _throttleCalculator = new ThrottleCalculator(
                    jobData.Configuration.ThrottledItemsPerSecond.Value,
                    jobData.Configuration.ThrottledMaxBurstSize.GetValueOrDefault());

            _taskQueueSemaphore = new SemaphoreSlim(
                jobData.Configuration.MaxConcurrentBatchesPerWorker,
                jobData.Configuration.MaxConcurrentBatchesPerWorker);

            _started = false;
            _terminated = false;
        }

        #region Health Check status properties

        private bool IsInRunningState => _lastStatus.State >= JobState.InProgress &&
                                         _lastStatus.State < JobState.Completed;

        private bool ShouldStartProcess => !_started && IsInRunningState;

        private bool IsProcessStalled
        {
            get
            {
                if (!IsProcessRunning)
                    return false;

                if (!_jobData.Configuration.MaxBlockedSecondsPerCycle.HasValue)
                    return false;

                var ticksBeforeConsideredAsStalled = Math.Max(
                    MinimumWaitMillisBeforeConsideredAsStalled * TimeSpan.TicksPerMillisecond,
                    _jobData.Configuration.MaxBlockedSecondsPerCycle.Value * TimeSpan.TicksPerSecond
                );

                return _statistics.TicksSinceLastIteration > ticksBeforeConsideredAsStalled ||
                       _statistics.TicksBetweenLastProcessStartAndFinish > ticksBeforeConsideredAsStalled;
            }
        }

        private bool IsExpired => !_jobData.Configuration.IsIndefinite &&
                                  _jobData.Configuration.ExpiresAt.HasValue &&
                                  DateTime.UtcNow > _jobData.Configuration.ExpiresAt.Value;

        private async Task<bool> CheckIfJobCanBeMarkedAsComplete()
        {
            if (_jobData.Configuration.IsIndefinite)
                return false;

            var queue = _composer.GetComponent<IJobQueue<TJobStep>>(_jobData.Configuration.QueueDescriptor.QueueType);

            if (await queue.GetQueueLength(_jobId) > 0)
                return false;

            // JobRunnerManager always runs preprocessor tasks before running a task. So, it will siffice
            // to check if the preprocessor job is in memory and not terminated
            if (_jobData.Configuration.PreprocessorJobIds.SafeAny(jid => _jobRunnerManager.IsJobRunnerActive(jid)))
                return false;
            
            var idleSecondsToCompletion = _jobData.Configuration.IdleSecondsToCompletion ??
                                          DefaultIdleSecondsToCompletion;
            var ticksBeforeConsideredAsCompleted = idleSecondsToCompletion * TimeSpan.TicksPerSecond;

            if (DateTime.UtcNow.Ticks - _lastStatus.LastProcessStartTime.Ticks <= ticksBeforeConsideredAsCompleted)
                return false;
            
            // Make sure we have the most up-to-date stats from store
            await RefreshData();
            if (DateTime.UtcNow.Ticks - _lastStatus.LastProcessStartTime.Ticks <= ticksBeforeConsideredAsCompleted)
                return false;

            return true;
        }

        #endregion

        #region Health check methods

        public async Task<bool> CheckHealth()
        {
            Log.Debug($"Job runner {_jobId} - Health check starting...");

            await RefreshData();
            _statistics.ReportStartingHealthCheck();

            try
            {
                if (ShouldStartProcess)
                {
                    Log.Debug($"Job runner {_jobId} - Health check: we should start processing");
                    StartProcess();
                    return true;
                }

                if (IsProcessStalled)
                {
                    Log.Warn($"Job runner {_jobId} - Health check: stalled processing detected");
                    return false;
                }

                if (!IsInRunningState)
                {
                    Log.Debug($"Job runner {_jobId} - Health check: everything looks okay");
                    return true;
                }

                if (IsProcessTerminated)
                {
                    Log.Warn($"Job runner {_jobId} - Health check: unexpected termination detected.");
                    return false;
                }

                if (IsExpired)
                {
                    Log.Debug($"Job runner {_jobId} - Health check: expiration detected");
                    await TerminateAsExpired();
                    return true;
                }

                if (await CheckIfJobCanBeMarkedAsComplete())
                {
                    Log.Debug($"Job runner {_jobId} - Health check: completion detected");
                    await TerminateAsComplete();
                }
            }
            catch (Exception e)
            {
                Log.Warn($"Job runner {_jobId} - Uncaught exception during health check", e);
                _statistics.ReportHealthCheckException(e);
            }

            return true;
        }

        public void StopRunner()
        {
            _stopping = true;
        }

        private async Task RefreshData()
        {
            _lastStatus = await _jobStore.LoadStatus(_tenantId, _jobId);
            Log.Debug($"Job runner {_jobId} - refresh complete");
        }

        private async Task TerminateAsExpired()
        {
            Log.Info($"Job runner {_jobId} - Setting job state to Expired");
            if (await _jobStore.UpdateState(_tenantId, _jobId, _lastStatus.State, JobState.Expired))
                await _jobNotification.NotifyJobUpdated(_jobId);
        }

        private async Task TerminateAsComplete()
        {
            Log.Info($"Job runner {_jobId} - Setting job state to Completed");
            if (await _jobStore.UpdateState(_tenantId, _jobId, _lastStatus.State, JobState.Completed))
                await _jobNotification.NotifyJobUpdated(_jobId);
        }

        private void StartProcess()
        {
            // Enqueue the work to be run in the background, so not awaiting
            Task.Run(Process);
        }

        #endregion

        private async Task Process()
        {
            if (!_initialized)
            {
                Log.Error($"Job runner {_jobId} - Starting to process a JobRunner that has not yet been initialized");
                return;
            }

            Log.Debug($"Job runner {_jobId} - Process started");
            _started = true;
            _throttleCalculator?.Start();

            try
            {
                await ProcessLoop();
            }
            catch (Exception e)
            {
                Log.Error($"Job runner {_jobId} - Fatal uncaught exception in ProcessLoop, dropping the process.", e);
                _statistics.ReportFatalException(e);

                Log.Debug($"Job runner {_jobId} - Fatal exception reported into statistics");
            }
            finally
            {
                _terminated = true;
                Log.Debug($"Job runner {_jobId} - terminated");

                try
                {
                    // Try to flush statistics, even if exception occured in catch block
                    _statistics.Flush();
                    Log.Debug($"Job runner {_jobId} - Flush statistics completed");
                }
                catch (Exception e)
                {
                    Log.Error($"Job runner {_jobId} - Could not flush statistics due to exception", e);
                }
            }
        }

        private async Task ProcessLoop()
        {
            while (true)
            {
                var loopWaitTime = 0;

                try
                {
                    if (_stopping)
                        break;
                    
                    if (_lastStatus.State == JobState.Paused)
                    {
                        await WaitUntilUnpaused();
                        continue;
                    }

                    if (_lastStatus.State >= JobState.Completed)
                        break;

                    loopWaitTime = await ProcessNextBatch();
                }
                catch (Exception e)
                {
                    Log.Debug($"Job runner {_jobId} - Iteration resulted in exception", e);
                    _statistics.ReportIterationException(e);
                }

                if (loopWaitTime >= 0 && !_stopping)
                {
                    loopWaitTime = Math.Min(MaximumWaitMillis, Math.Max(MinimumWaitMillis, loopWaitTime));
                    await Task.Delay(loopWaitTime);
                    // TODO: Replace by something to get notified when new items arrive
                }
            }
        }

        private async Task<int> ProcessNextBatch()
        {
            var throttledBatchSize = _throttleCalculator?.AvailableItems ?? _jobData.Configuration.MaxBatchSize;
            if (throttledBatchSize <= 0)
                return _throttleCalculator?.WaitTimeForNextMillis ?? 0;

            if (!await EnterTaskQueueSemaphore())
            {
                Log.Debug($"Job runner {_jobId} - Could not start batch, task queue semaphore timed out");
                return -1;
            }

            _statistics.ReportIterationStarted();
            List<TJobStep> steps;

            try
            {
                if (await CheckTargetQueueLengthExceeded())
                {
                    Log.Debug($"Job runner {_jobId} - Target queue size exceeds limit");
                    LeaveTaskQueueSemaphore();
                    return WaitMillisWhenTargetQueueIsFull;
                }

                if (IsExpired)
                {
                    Log.Debug($"Job runner {_jobId} - Job is already expired, not processing");
                    LeaveTaskQueueSemaphore();
                    return WaitMillisWhenJobIsExpired;
                }

                // The state might have been changed after waiting for the TaskQueueSemaphore
                if (_lastStatus.State != JobState.InProgress && _lastStatus.State != JobState.Draining)
                {
                    Log.Debug($"Job runner {_jobId} - Job is not in a runnable state, not processing");
                    LeaveTaskQueueSemaphore();
                    return 0;
                }
                
                // Check if stopping right before dequeue. If we dequeue something, we have to process it.
                if (_stopping)
                    return -1;
            
                _statistics.ReportDequeueAttempt();

                var nextBatchSize = Math.Min(throttledBatchSize, _jobData.Configuration.MaxBatchSize);

                var queue = _composer.GetComponent<IJobQueue<TJobStep>>(_jobData.Configuration.QueueDescriptor.QueueType);

                steps = (await queue.DequeueBatch(nextBatchSize, _jobId)).SafeToList();
                if (steps == null || steps.Count <= 0)
                {
                    Log.Debug($"Job runner {_jobId} - There's no more work to do");

                    if (await CheckIfJobCanBeMarkedAsComplete())
                    {
                        await TerminateAsComplete();
                    }
                    
                    LeaveTaskQueueSemaphore();
                    return WaitMillisWhenThereIsNoMoreWork;
                }

                if (_lastStatus.State == JobState.Draining)
                {
                    LeaveTaskQueueSemaphore();
                    return -1;
                }
                
                _throttleCalculator?.DecreaseQuota(steps.Count);
                _statistics.ReportProcessStarted();
            }
            catch (Exception)
            {
                // Catching all exceptions to return semaphore to original state
                LeaveTaskQueueSemaphore();

                // Not logging anything here. Instead, rethrow and allow caller to react.
                throw;
            }

            // ReSharper disable once UnusedVariable
            // Not awaiting on the returned task, to allow running multiple tasks in parallel
            var enqueuedTask = Task.Run(async () =>
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    var processingResult = await _processor.Process(steps);

                    stopwatch.Stop();
                    _statistics.ReportProcessFinished(steps.Count, stopwatch.Elapsed, processingResult);
                }
                catch (Exception e)
                {
                    Log.Warn($"Job runner {_jobId} - Uncaught exception while process invokation", e);
                    _statistics.ReportProcessorInvocationException(e);
                }
                finally
                {
                    LeaveTaskQueueSemaphore();
                }
            });

            return -1;
        }

        private async Task WaitUntilUnpaused()
        {
            Log.Debug($"Job runner {_jobId} - Starting to loop in paused state");

            while (_lastStatus.State == JobState.Paused)
            {
                await Task.Delay(WaitMillisWhenJobIsPaused);
                _statistics.ReportPausedIterationFinished();
                
                if (_stopping)
                    break;
            }

            Log.Debug($"Job runner {_jobId} - Not paused anymore, returning back to main loop");
        }

        private Task<bool> EnterTaskQueueSemaphore()
        {
            try
            {
                return _taskQueueSemaphore.WaitAsync(WaitMillisForTaskSemaphore);
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(false);
            }
        }

        private void LeaveTaskQueueSemaphore()
        {
            try
            {
                _taskQueueSemaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                Log.Warn($"Job runner {_jobId} - Semaphore is full, but it's still being released. Possibly a leak.");
                // Ignore the exception
            }
        }

        private async Task<bool> CheckTargetQueueLengthExceeded()
        {
            var maxLength = _jobData.Configuration.MaxTargetQueueLength.GetValueOrDefault();
            if (maxLength <= 0)
                return false;

            var targetLength = await _processor.GetTargetQueueLength();
            return targetLength > maxLength;
        }
    }
}