using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobStepSource
    {
        void Initialize(string jobId = null);
        Task EnsureJobSourceExists();
        Task<bool> Any();
        Task Purge();
    }

    [Contract]
    public interface IJobStepSource<TItem> : IJobStepSource where TItem : IJobStep
    {
        Task<TItem> GetNext();
        Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize);
    }
}