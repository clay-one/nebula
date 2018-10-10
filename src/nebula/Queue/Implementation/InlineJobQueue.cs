using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using hydrogen.General.Utils;

namespace Nebula.Queue.Implementation
{
    [Component]
    [IgnoredOnAssemblyRegistration]
    public class InlineJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        [ComponentPlug]
        public IJobProcessor<TItem> Processor { get; set; }

        public void Initialize(string jobId = null)
        {

        }

        public Task<long> GetQueueLength(string jobId = null)
        {
            return Task.FromResult(0L);
        }

        public async Task Enqueue(TItem item, string jobId = null)
        {
            await Processor.Process(item.Yield().ToList());
        }

        public async Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
        {
            await Processor.Process(items.ToList());
        }
        
        public Task EnsureJobSourceExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any(string jobId = null)
        {
            return Task.FromResult(false);
        }

        public Task Purge(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<TItem> GetNext(string jobId = null)
        {
            return Task.FromResult(default(TItem));
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize, string jobId = null)
        {
            return Task.FromResult(Enumerable.Empty<TItem>());
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