using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobQueue
    {
        bool QueueExistenceChecked { get; set; }

        Task EnsureJobQueueExists(string jobId = null);
        Task<long> GetQueueLength(string jobId = null);
        Task PurgeQueueContents(string jobId = null);
    }

    [Contract]
    public interface IJobQueue<TItem> : IJobQueue where TItem : IJobStep
    {
        Task Enqueue(TItem item, string jobId = null);
        Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null);

        Task<TItem> Dequeue(string jobId = null);
        Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null);
    }
}