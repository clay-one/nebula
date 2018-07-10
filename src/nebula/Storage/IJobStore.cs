using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Storage.Model;

namespace Nebula.Storage
{
    [Contract]
    public interface IJobStore
    {
        Task<List<JobData>> LoadAll(string tenantId);
        Task<JobData> Load(string tenantId, string jobId);
        Task<JobStatusData> LoadStatus(string tenantId, string jobId);
        
        Task<JobData> LoadFromAnyTenant(string jobId);
        Task<List<string>> LoadAllRunnableIdsFromAnyTenant();
        
        Task<JobData> AddOrUpdateDefinition(JobData jobData);
        Task<bool> UpdateState(string tenantId, string jobId, JobState? expectedState, JobState newState);
        Task UpdateStatus(string tenantId, string jobId, JobStatusUpdateData change);
        Task AddException(string tenantId, string jobId, JobStatusErrorData jobStatusErrorData);
        Task<bool> AddPredecessor(string tenantId, string jobId, string predecessorJobId);
    }
}