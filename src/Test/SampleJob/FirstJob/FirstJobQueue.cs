using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula.Queue;

namespace Test.SampleJob.FirstJob
{
    public class FirstJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        private readonly Dictionary<string, TItem> _queue = new Dictionary<string, TItem>();
        private string _jobId;
        public bool QueueExistenceChecked { get; set; }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            return Task.FromResult((long) _queue.Count);
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
        
        public Task EnsureJobSourceExists(string jobId = null)
        {
            QueueExistenceChecked = true;
            return Task.CompletedTask;
        }

        public Task<bool> Any(string jobId = null)
        {
            return Task.FromResult((long) _queue.Count > 0);
        }

        public Task Purge(string jobId = null)
        {
            _queue.Clear();
            return Task.CompletedTask;
        }

        public Task<TItem> GetNext(string jobId = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize, string jobId = null)
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

        public Task<TItem> Dequeue(string jobId = null)
        {
            return GetNext(jobId);
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return GetNextBatch(maxBatchSize, jobId);
        }

        #endregion
    }
}