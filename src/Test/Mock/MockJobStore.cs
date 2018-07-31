using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Test.Mock
{
    [Component]
    public class MockJobStore : IJobStore
    {
        private readonly Dictionary<string, JobData> _jobs = new Dictionary<string, JobData>();

        public Task<List<JobData>> LoadAll(string tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<JobData> Load(string tenantId, string jobId)
        {
            throw new NotImplementedException();
        }

        public Task<JobStatusData> LoadStatus(string tenantId, string jobId)
        {
            throw new NotImplementedException();
        }

        public async Task<JobData> LoadFromAnyTenant(string jobId)
        {
            return _jobs[jobId];
        }

        public Task<List<string>> LoadAllRunnableIdsFromAnyTenant()
        {
            throw new NotImplementedException();
        }

        public async Task<JobData> AddOrUpdateDefinition(JobData jobData)
        {
            _jobs.Add(jobData.JobId,jobData);
            return jobData;
        }

        public Task<bool> UpdateState(string tenantId, string jobId, JobState? expectedState, JobState newState)
        {
            throw new NotImplementedException();
        }

        public Task UpdateStatus(string tenantId, string jobId, JobStatusUpdateData change)
        {
            throw new NotImplementedException();
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