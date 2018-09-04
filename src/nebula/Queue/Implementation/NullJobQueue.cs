using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue.Implementation
{
    [Component]
    [IgnoredOnAssemblyRegistration]
    public class NullJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        public bool QueueExistenceChecked { get; set; }

        public Task EnsureJobQueueExists(string jobId = null)
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            return Task.FromResult(0L);
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task Enqueue(TItem item, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<TItem> Dequeue(string jobId = null)
        {
            return Task.FromResult(default(TItem));
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return Task.FromResult(Enumerable.Empty<TItem>());
        }
    }
}