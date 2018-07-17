using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nebula.Queue;

namespace SampleJob
{
  public   class SampleJobQueue : IJobQueue<SampleJobStep>
    {
        public Task EnsureJobQueueExists(string jobId = null)
        {
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

        public Task Enqueue(SampleJobStep item, string jobId = null)
        {
            throw new NotImplementedException();
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
