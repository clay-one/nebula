using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace SampleJob
{
    public class SampleJobQueue<TItem> : IJobQueue<SampleJobStep>
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

        public Task Enqueue(SampleJobStep item)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<SampleJobStep> items)
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
            return Task.CompletedTask;
        }

        public Task<SampleJobStep> GetNext()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SampleJobStep>> GetNextBatch(int maxBatchSize)
        {
            throw new NotImplementedException();
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

        public Task<SampleJobStep> Dequeue(string jobId = null)
        {
            return GetNext();
        }

        public Task<IEnumerable<SampleJobStep>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return GetNextBatch(maxBatchSize);
        }

        #endregion
    }
}