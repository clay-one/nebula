using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Test.Mock
{
    [Component]
    public class MockJobStore : IJobStore
    {
        private readonly Dictionary<string, Dictionary<string, JobData>> _jobs =
            new Dictionary<string, Dictionary<string, JobData>>();

        public Task<List<JobData>> LoadAll(string tenantId)
        {
            throw new NotImplementedException();
        }

        public async Task<JobData> Load(string tenantId, string jobId)
        {
            _jobs.TryGetValue(tenantId, out var value);
            return value?[jobId];
        }

        public async Task<JobStatusData> LoadStatus(string tenantId, string jobId)
        {
            var job = await Load(tenantId, jobId);

            return job.Status;
        }

        public async Task<JobData> LoadFromAnyTenant(string jobId)
        {
            return _jobs.Values.Where(a => a.ContainsKey(jobId)).Select(datas => datas[jobId]).SingleOrDefault();
        }

        public Task<List<string>> LoadAllRunnableIdsFromAnyTenant()
        {
            throw new NotImplementedException();
        }

        public async Task<JobData> AddOrUpdateDefinition(JobData jobData)
        {
            if (!_jobs.ContainsKey(jobData.TenantId))
                _jobs[jobData.TenantId] = new Dictionary<string, JobData>();

            _jobs[jobData.TenantId].Add(jobData.JobId, jobData);

            return jobData;
        }

        public Task<bool> UpdateState(string tenantId, string jobId, JobState? expectedState, JobState newState)
        {
            _jobs.TryGetValue(tenantId, out var value);
            var job = value?[jobId];
            if (job == null)
                return Task.FromResult(false);

            job.Status.State = newState;

            return Task.FromResult(true);
        }

        public async Task UpdateStatus(string tenantId, string jobId, JobStatusUpdateData change)
        {
            var job = await Load(tenantId, jobId);

            job.Status.LastIterationStartTime = job.Status.LastIterationStartTime > change.LastIterationStartTime
                ? job.Status.LastIterationStartTime
                : change.LastIterationStartTime;
            job.Status.LastDequeueAttemptTime = job.Status.LastDequeueAttemptTime > change.LastDequeueAttemptTime
                ? job.Status.LastDequeueAttemptTime
                : change.LastDequeueAttemptTime;
            job.Status.LastProcessStartTime = job.Status.LastProcessStartTime > change.LastProcessStartTime
                ? job.Status.LastProcessStartTime
                : change.LastProcessStartTime;
            job.Status.LastProcessFinishTime = job.Status.LastProcessFinishTime > change.LastProcessFinishTime
                ? job.Status.LastProcessFinishTime
                : change.LastProcessFinishTime;
            job.Status.LastHealthCheckTime = job.Status.LastHealthCheckTime > change.LastHealthCheckTime
                ? job.Status.LastHealthCheckTime
                : change.LastHealthCheckTime;
            job.Status.ItemsProcessed += change.ItemsProcessedDelta;
            job.Status.ItemsRequeued += change.ItemsRequeuedDelta;
            job.Status.ItemsGeneratedForTargetQueue += change.ItemsGeneratedForTargetQueueDelta;
            job.Status.ProcessingTimeTakenMillis += change.ProcessingTimeTakenMillisDelta;
            job.Status.ItemsFailed += change.ItemsFailedDelta;

            if (change.LastFailTime.HasValue)
                job.Status.LastFailTime = job.Status.LastFailTime > change.LastFailTime
                    ? job.Status.LastFailTime
                    : change.LastFailTime.Value;

            if (change.LastFailures != null && change.LastFailures.Length > 0)
            {
                foreach (var jobStatusErrorData in change.LastFailures)
                    job.Status.LastFailures.Append(jobStatusErrorData);
            }
        }

        public Task AddException(string tenantId, string jobId, JobStatusErrorData jobStatusErrorData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddPredecessor(string tenantId, string jobId, string predecessorJobId)
        {
            throw new NotImplementedException();
        }
    }
}