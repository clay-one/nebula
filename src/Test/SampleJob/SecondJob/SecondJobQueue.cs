using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace Test.SampleJob.SecondJob
{
    public class SecondJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        private string _jobId;
        public bool QueueExistenceChecked { get; set; }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
        }

        public Task<long> GetQueueLength()
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(TItem item)
        {
            throw new NotImplementedException();
        }

        public Task EnqueueBatch(IEnumerable<TItem> items)
        {
            throw new NotImplementedException();
        }

        public Task EnsureJobSourceExists()
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<bool> Any()
        {
            throw new NotImplementedException();
        }

        public Task Purge()
        {
            throw new NotImplementedException();
        }

        public Task<TItem> GetNext()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            throw new NotImplementedException();
        }
    }
}