using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using ComposerCore.Cache;

namespace Nebula.Queue.Implementation
{
    [Contract]
    [Component]
    [ComponentCache(typeof(ContractAgnosticComponentCache))]
    [IgnoredOnAssemblyRegistration]
    public class InMemoryJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        private readonly Dictionary<string, Queue<TItem>> _queueContents;
        private readonly object _lockObject;

        public InMemoryJobQueue()
        {
            _queueContents = new Dictionary<string, Queue<TItem>>();
            _lockObject = new object();
        }

        public Task EnsureJobQueueExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            lock (_lockObject)
            {
                return Task.FromResult((long)GetQueue(jobId).Count);
            }
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            lock (_lockObject)
            {
                GetQueue(jobId).Clear();
                return Task.CompletedTask;
            }
        }

        public Task Enqueue(TItem item, string jobId = null)
        {
            lock (_lockObject)
            {
                GetQueue(jobId).Enqueue(item);
            }

            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
        {
            lock (_lockObject)
            {
                GetQueue(jobId).EnqueueAll(items);
            }

            return Task.CompletedTask;
        }

        public Task<TItem> Dequeue(string jobId = null)
        {
            lock (_lockObject)
            {
                var queue = GetQueue(jobId);
                return Task.FromResult(queue.Count > 0 ? queue.Dequeue() : default(TItem));
            }
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            lock (_lockObject)
            {
                var queue = GetQueue(jobId);
                return Task.FromResult(Enumerable.Range(0, maxBatchSize)
                    .Select(_ => queue.Count > 0 ? queue.Dequeue() : default(TItem))
                    .Where(item => item != null)
                    .ToList().AsEnumerable());
            }
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        private Queue<TItem> GetQueue(string jobId)
        {
            if (_queueContents.TryGetValue(jobId ?? "", out var queue)) 
                return queue;
            
            queue = new Queue<TItem>();
            _queueContents[jobId ?? ""] = queue;

            return queue;
        }
    }
}