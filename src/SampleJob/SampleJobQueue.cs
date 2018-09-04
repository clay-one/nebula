using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace SampleJob
{
    public class SampleJobQueue<TItem> : IJobQueue<SampleJobStep>
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
            return Task.CompletedTask;
        }

        public Task Enqueue(SampleJobStep item, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<SampleJobStep> items, string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<SampleJobStep> Dequeue(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SampleJobStep>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            throw new NotImplementedException();
        }
    }
}