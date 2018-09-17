using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace SampleJob
{
    public class SampleJobQueue<TItem> : IJobQueue<SampleJobStep>
    {
        public bool QueueExistenceChecked { get; set; }

        public Task<long> GetQueueLength(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(SampleJobStep item, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<SampleJobStep> items, string jobId = null)
        {
            throw new NotImplementedException();
        }
        
        public Task EnsureJobSourceExists(string jobId = null)
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<bool> Any(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task Purge(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<SampleJobStep> GetNextStep(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SampleJobStep>> GetNextStepsBatch(int maxBatchSize, string jobId = null)
        {
            throw new NotImplementedException();
        }

        #region Obsolete members

        public Task EnsureJobQueueExists(string jobId = null)
        {
            return EnsureJobSourceExists(jobId);
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            return Purge(jobId);
        }

        public Task<SampleJobStep> Dequeue(string jobId = null)
        {
            return GetNextStep(jobId);
        }

        public Task<IEnumerable<SampleJobStep>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return GetNextStepsBatch(maxBatchSize, jobId);
        }

        #endregion
    }
}