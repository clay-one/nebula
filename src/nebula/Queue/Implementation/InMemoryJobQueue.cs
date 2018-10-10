using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using ComposerCore.Cache;
using hydrogen.General.Collections;

namespace Nebula.Queue.Implementation
{
    [Contract]
    [Component]
    [ComponentCache(typeof(ContractAgnosticComponentCache))]
    [IgnoredOnAssemblyRegistration]
    public class InMemoryJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        private readonly object _lockObject;
        private readonly Dictionary<string, Queue<TItem>> _queueContents;
        private string _jobId;

        public InMemoryJobQueue()
        {
            _queueContents = new Dictionary<string, Queue<TItem>>();
            _lockObject = new object();
        }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
        }

        public Task<long> GetQueueLength()
        {
            lock (_lockObject)
            {
                return Task.FromResult((long) GetQueue().Count);
            }
        }

        public Task Enqueue(TItem item)
        {
            lock (_lockObject)
            {
                GetQueue().Enqueue(item);
            }

            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items)
        {
            lock (_lockObject)
            {
                GetQueue().EnqueueAll(items);
            }

            return Task.CompletedTask;
        }
        
        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any()
        {
            lock (_lockObject)
            {
                var queueLength = (long) GetQueue().Count;
                return Task.FromResult(queueLength > 0);
            }
        }

        public Task Purge()
        {
            lock (_lockObject)
            {
                GetQueue().Clear();
                return Task.CompletedTask;
            }
        }

        public Task<TItem> GetNext()
        {
            lock (_lockObject)
            {
                var queue = GetQueue();
                return Task.FromResult(queue.Count > 0 ? queue.Dequeue() : default(TItem));
            }
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            lock (_lockObject)
            {
                var queue = GetQueue();
                return Task.FromResult(Enumerable.Range(0, maxBatchSize)
                    .Select(_ => queue.Count > 0 ? queue.Dequeue() : default(TItem))
                    .Where(item => item != null)
                    .ToList().AsEnumerable());
            }
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        private Queue<TItem> GetQueue()
        {
            if (_queueContents.TryGetValue(_jobId ?? "", out var queue))
                return queue;

            queue = new Queue<TItem>();
            _queueContents[_jobId ?? ""] = queue;

            return queue;
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