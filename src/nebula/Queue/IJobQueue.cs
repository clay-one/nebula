using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobQueue
    {
        Task<long> GetQueueLength(string jobId = null);

        [Obsolete("Use EnsureJobSourcExists instead")]
        Task EnsureJobQueueExists(string jobId = null);

        [Obsolete("Use PurgeContents instead")]
        Task PurgeQueueContents(string jobId = null);
    }

    [Contract]
    public interface IJobQueue<TItem> : IJobStepSource<TItem>, IJobQueue where TItem : IJobStep
    {
        Task Enqueue(TItem item, string jobId = null);
        Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null);

        [Obsolete("Use GetNextStep Instead")]
        Task<TItem> Dequeue(string jobId = null);

        [Obsolete("Use GetNextStepBatch Instead")]
        Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null);
    }
}