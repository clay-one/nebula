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
        public void Initialize(string jobId = null)
        {
        }

        public Task<long> GetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public Task Enqueue(TItem item)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items)
        {
            return Task.CompletedTask;
        }

        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any()
        {
            return Task.FromResult(false);
        }

        public Task Purge()
        {
            return Task.CompletedTask;
        }

        public Task<TItem> GetNext()
        {
            return Task.FromResult(default(TItem));
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            return Task.FromResult(Enumerable.Empty<TItem>());
        }

        #region Obsolete members

        public Task EnsureJobQueueExists(string jobId = null)
        {
            return EnsureJobSourceExists();
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            return Purge();
        }

        public Task<TItem> Dequeue(string jobId = null)
        {
            return GetNext();
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return GetNextBatch(maxBatchSize);
        }

        #endregion
    }
}