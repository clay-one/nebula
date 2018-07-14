using System;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Attributes;
using hydrogen.General.Collections;
using hydrogen.General.Text;
using hydrogen.General.Validation;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Job.Implementation
{
    [Component]
    public class DefaultJobManager : IJobManager
    {
        [ComponentPlug]
        public IJobNotification JobNotification { get; set; }

        [ComponentPlug]
        public IJobStore JobStore { get; set; }

        [ComponentPlug]
        public IComposer Composer { get; set; }
        
        public Task CleanupOldJobs()
        {
            return Task.CompletedTask;
        }

        public async Task<string> CreateNewJobOrUpdateDefinition<TJobStep>(string tenantId, 
            string jobDisplayName = null, string jobId = null, JobConfigurationData configuration = null) 
            where TJobStep : IJobStep
        {
            if (string.IsNullOrWhiteSpace(jobId))
                jobId = Base32Url.ToBase32String(Guid.NewGuid().ToByteArray());
            configuration = configuration ?? new JobConfigurationData();

            ValidateAndFixConfiguration(configuration);
            
            var job = new JobData
            {
                JobId = jobId,
                TenantId = tenantId,
                JobDisplayName = jobDisplayName ?? typeof(TJobStep).Name,
                JobStepType = typeof(TJobStep).AssemblyQualifiedName,
                CreationTime = DateTime.UtcNow,
                CreatedBy = "unknown",
                Configuration = configuration,
                Status = new JobStatusData
                {
                    State = JobState.Initializing,
                    StateTime = DateTime.UtcNow,
                    LastIterationStartTime = DateTime.UtcNow,
                    LastDequeueAttemptTime = DateTime.UtcNow,
                    LastProcessStartTime = DateTime.UtcNow,
                    LastProcessFinishTime = DateTime.UtcNow,
                    LastHealthCheckTime = DateTime.UtcNow,
                    ItemsProcessed = 0,
                    ItemsRequeued = 0,
                    ItemsGeneratedForTargetQueue = 0,
                    EstimatedTotalItems = -1,
                    ProcessingTimeTakenMillis = 0,
                    ItemsFailed = 0,
                    LastFailTime = null,
                    LastFailures = new JobStatusErrorData[0],
                    ExceptionCount = 0,
                    LastExceptionTime = null,
                    LastExceptions = new JobStatusErrorData[0]
                }
            };

            await JobStore.AddOrUpdateDefinition(job);
            await Composer.GetComponent<IJobQueue<TJobStep>>().EnsureJobQueueExists(jobId);

            return jobId;
        }

        public async Task AddPredecessor(string tenantId, string jobId, string predecessorJobId)
        {
            await JobStore.AddPredecessor(tenantId, jobId, predecessorJobId);
        }

        public async Task StartJob(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                throw new InvalidOperationException($"JobId {jobId} does not exist.");

            if (jobData.Status.State != JobState.Initializing)
                throw new InvalidOperationException($"JobId {jobId} cannot be started due to its state.");

            if (!await JobStore.UpdateState(tenantId, jobId, JobState.Initializing, JobState.InProgress))
                throw new InvalidOperationException($"JobId {jobId} could not be updated to start.");
            
            await JobNotification.NotifyJobUpdated(jobId);
        }

        public async Task StartJobIfNotStarted(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                throw new InvalidOperationException($"JobId {jobId} does not exist.");

            if (jobData.Status.State != JobState.Initializing)
                return;

            await JobStore.UpdateState(tenantId, jobId, JobState.Initializing, JobState.InProgress);
            await JobNotification.NotifyJobUpdated(jobId);
        }

        public async Task<ApiValidationResult> StopJob(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobId);
            
            if (jobData.Status.State == JobState.Stopped)
                return ApiValidationResult.Ok();

            if (jobData.Configuration.IsIndefinite)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobAction);
            
            if (jobData.Configuration.PreprocessorJobIds.SafeAny())
            {
                foreach (var preprocessorJobId in jobData.Configuration.PreprocessorJobIds)
                {
                    var preprocessorJobStatus = await JobStore.LoadStatus(tenantId, preprocessorJobId);
                    if (preprocessorJobStatus.State < JobState.Completed)
                        return ApiValidationResult.Failure(ErrorKeys.JobActionHasPreprocessorDependency,
                            new[] {preprocessorJobId});
                }
            }
            
            var changeableStates = new[] {JobState.InProgress, JobState.Draining, JobState.Paused};
            if (changeableStates.Any(s => s == jobData.Status.State))
            {
                var updated = await JobStore.UpdateState(tenantId, jobId, jobData.Status.State, JobState.Stopped);
                if (updated)
                {
                    await JobNotification.NotifyJobUpdated(jobId);
                    
                    var jobQueue = GetJobQueue(jobData.JobStepType);
                    if (jobQueue != null)
                        await jobQueue.PurgeQueueContents(jobId);
                    
                    return ApiValidationResult.Ok();
                }
            }
            
            return ApiValidationResult.Failure(ErrorKeys.InvalidJobState);
        }

        public async Task<ApiValidationResult> PauseJob(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobId);
            
            if (jobData.Status.State == JobState.Paused)
                return ApiValidationResult.Ok();

            var changeableStates = new[] {JobState.InProgress, JobState.Draining};
            if (changeableStates.Any(s => s == jobData.Status.State))
            {
                var updated = await JobStore.UpdateState(tenantId, jobId, jobData.Status.State, JobState.Paused);
                if (updated)
                {
                    await JobNotification.NotifyJobUpdated(jobId);
                    return ApiValidationResult.Ok();
                }
            }
            
            return ApiValidationResult.Failure(ErrorKeys.InvalidJobState);
        }

        public async Task<ApiValidationResult> DrainJob(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobId);
            
            if (jobData.Status.State == JobState.Draining)
                return ApiValidationResult.Ok();

            var changeableStates = new[] {JobState.InProgress, JobState.Paused};
            if (changeableStates.Any(s => s == jobData.Status.State))
            {
                var updated = await JobStore.UpdateState(tenantId, jobId, jobData.Status.State, JobState.Draining);
                if (updated)
                {
                    await JobNotification.NotifyJobUpdated(jobId);
                    return ApiValidationResult.Ok();
                }
            }
            
            return ApiValidationResult.Failure(ErrorKeys.InvalidJobState);
        }

        public async Task<ApiValidationResult> ResumeJob(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobId);
            
            if (jobData.Status.State == JobState.InProgress)
                return ApiValidationResult.Ok();

            var changeableStates = new[] {JobState.Draining, JobState.Paused};
            if (changeableStates.Any(s => s == jobData.Status.State))
            {
                var updated = await JobStore.UpdateState(tenantId, jobId, jobData.Status.State, JobState.InProgress);
                if (updated)
                {
                    await JobNotification.NotifyJobUpdated(jobId);
                    return ApiValidationResult.Ok();
                }
            }
            
            return ApiValidationResult.Failure(ErrorKeys.InvalidJobState);
        }

        public async Task<ApiValidationResult> PurgeJobQueue(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return ApiValidationResult.Failure(ErrorKeys.InvalidJobId);
            
            var jobQueue = GetJobQueue(jobData.JobStepType);
            if (jobQueue == null)
                return ApiValidationResult.Failure(ErrorKeys.UnknownInternalServerError);

            await jobQueue.PurgeQueueContents(jobId);
            return ApiValidationResult.Ok();
        }

        public async Task<long> GetQueueLength(string tenantId, string jobId)
        {
            var jobQueue = GetJobQueue((await JobStore.Load(tenantId, jobId))?.JobStepType);
            return jobQueue == null ? 0 : await jobQueue.GetQueueLength(jobId);
        }

        #region Private helper methods
        
        private void ValidateAndFixConfiguration(JobConfigurationData configuration)
        {
            configuration.MaxBatchSize = Math.Max(1, Math.Min(1000, configuration.MaxBatchSize));
            configuration.MaxConcurrentBatchesPerWorker =
                Math.Max(1, Math.Min(10000, configuration.MaxConcurrentBatchesPerWorker));

            if (configuration.ThrottledItemsPerSecond.HasValue && configuration.ThrottledItemsPerSecond <= 0d)
                throw new ArgumentException("Throttle speed cannot be zero or negative");
            if (configuration.ThrottledItemsPerSecond.HasValue)
                configuration.ThrottledItemsPerSecond = Math.Max(0.001d, configuration.ThrottledItemsPerSecond.Value);

            if (configuration.ThrottledMaxBurstSize.HasValue && configuration.ThrottledMaxBurstSize <= 0)
                throw new ArgumentException("Throttle burst size cannot be zero or negative");
            if (configuration.ThrottledMaxBurstSize.HasValue)
                configuration.ThrottledMaxBurstSize = Math.Max(1, configuration.ThrottledMaxBurstSize.Value);
            
            if (configuration.ExpiresAt.HasValue && configuration.ExpiresAt < DateTime.Now)
                throw new ArgumentException("Job is already expired and cannot be added");

            if(configuration.QueueDescriptor == null)
                throw new ArgumentException("QueueDescriptor must be specified in job configuration");

            if (configuration.IdleSecondsToCompletion.HasValue)
                configuration.IdleSecondsToCompletion = Math.Max(10, configuration.IdleSecondsToCompletion.Value);

            if (configuration.MaxBlockedSecondsPerCycle.HasValue)
                configuration.MaxBlockedSecondsPerCycle = Math.Max(30, configuration.MaxBlockedSecondsPerCycle.Value);

            if (configuration.MaxTargetQueueLength.HasValue)
                configuration.MaxTargetQueueLength = Math.Max(1, configuration.MaxTargetQueueLength.Value);
        }

        private IJobQueue GetJobQueue(string jobStepTypeName)
        {
            if (string.IsNullOrWhiteSpace(jobStepTypeName))
                return null;
            
            var stepType = Type.GetType(jobStepTypeName);
            if (stepType == null)
            {
                // TODO: Log error
                return null;
            }

            if (!(typeof(IJobStep)).IsAssignableFrom(stepType))
            {
                // TODO: Log error
                return null;
            }
            
            var contract = typeof(IJobQueue<>).MakeGenericType(stepType);
            if (!(Composer.GetComponent(contract) is IJobQueue jobQueue))
            {
                // TODO: Log error
                return null;
            }

            return jobQueue;
        }

        #endregion
        
    }
}