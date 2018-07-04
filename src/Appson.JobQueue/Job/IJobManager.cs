using System.Threading.Tasks;
using ComposerCore.Attributes;
using hydrogen.General.Validation;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace Nebula.Job
{
    [Contract]
    public interface IJobManager
    {
        Task CleanupOldJobs();

        Task<string> CreateNewJobOrUpdateDefinition<TJobStep>(string tenantId, 
            string jobDisplayName = null, string jobId = null, JobConfigurationData configuration = null) 
            where TJobStep : IJobStep;

        Task AddPredecessor(string tenantId, string jobId, string predecessorJobId);
        
        Task StartJob(string tenantId, string jobId);
        Task StartJobIfNotStarted(string tenantId, string jobId);
        Task<ApiValidationResult> StopJob(string tenantId, string jobId);
        Task<ApiValidationResult> PauseJob(string tenantId, string jobId);
        Task<ApiValidationResult> DrainJob(string tenantId, string jobId);
        Task<ApiValidationResult> ResumeJob(string tenantId, string jobId);
        Task<ApiValidationResult> PurgeJobQueue(string tenantId, string jobId);
        Task<long> GetQueueLength(string tenantId, string jobId);
    }
}