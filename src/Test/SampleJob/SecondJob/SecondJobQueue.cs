using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace Test.SampleJob.SecondJob
{
    public class SecondJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        public bool QueueExistenceChecked { get; set; }

        public Task EnsureJobQueueExists(string jobId = null)
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(TItem item, string jobId = null)
        {
            throw new NotImplementedException();
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