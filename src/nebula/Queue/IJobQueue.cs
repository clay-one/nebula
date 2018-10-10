using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobQueue
    {
        Task<long> GetQueueLength();

        [Obsolete("Use EnsureJobSourceExists instead")]
        Task EnsureJobQueueExists(string jobId = null);

        [Obsolete("Use Purge instead")]
        Task PurgeQueueContents(string jobId = null);
    }

    [Contract]
    public interface IJobQueue<TItem> : IJobStepSource<TItem>, IJobQueue where TItem : IJobStep
    {
        Task Enqueue(TItem item);
        Task EnqueueBatch(IEnumerable<TItem> items);

        [Obsolete("Use GetNext Instead")]
        Task<TItem> Dequeue(string jobId = null);

        [Obsolete("Use GetNextBatch Instead")]
        Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null);
    }
}