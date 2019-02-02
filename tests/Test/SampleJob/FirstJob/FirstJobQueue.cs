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
        public int InitializationCount { get; set; }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
            InitializationCount++;
        }

        public Task<long> GetQueueLength()
        {
            return Task.FromResult((long) _queue.Count);
        }

        public Task Enqueue(TItem item)
        {
            _queue.Add(_jobId, item);
            return Task.CompletedTask;
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
            return Task.FromResult((long) _queue.Count > 0);
        }

        public Task Purge()
        {
            _queue.Clear();
            return Task.CompletedTask;
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