using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace Test.SampleJob.FirstJob
{
    public class FirstJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        private readonly Dictionary<string, TItem> _queue = new Dictionary<string, TItem>();
        public bool QueueExistenceChecked { get; set; }

        public Task EnsureJobQueueExists(string jobId = null)
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            return Task.FromResult((long) _queue.Count);
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            _queue.Clear();
            return Task.CompletedTask;
        }

        public Task Enqueue(TItem item, string jobId = null)
        {
            _queue.Add(jobId, item);
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<TItem> Dequeue(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            throw new NotImplementedException();
        }
    }
}