using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobStepSource
    {
        Task EnsureJobSourcExists(string jobId = null);
        Task<bool> IsThereAnyMoreSteps(string jobId = null);
        Task PurgeContents(string jobId = null);
    }

    [Contract]
    public interface IJobStepSource<TItem> : IJobStepSource where TItem : IJobStep
    {
        Task<TItem> GetNextStep(string jobId = null);
        Task<IEnumerable<TItem>> GetNextStepBatch(int maxBatchSize, string jobId = null);
    }
}