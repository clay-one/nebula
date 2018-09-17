using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobStepSource
    {
        Task EnsureJobSourceExists(string jobId = null);
        Task<bool> Any(string jobId = null);
        Task Purge(string jobId = null);
    }

    [Contract]
    public interface IJobStepSource<TItem> : IJobStepSource where TItem : IJobStep
    {
        Task<TItem> GetNextStep(string jobId = null);
        Task<IEnumerable<TItem>> GetNextStepsBatch(int maxBatchSize, string jobId = null);
    }
}