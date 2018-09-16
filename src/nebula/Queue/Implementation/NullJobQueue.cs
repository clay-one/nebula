using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue.Implementation
{
    [Component]
    [IgnoredOnAssemblyRegistration]
    public class NullJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
    {
        public Task<long> GetQueueLength(string jobId = null)
        {
            return Task.FromResult(0L);
        }

        public Task Enqueue(TItem item, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task EnsureJobSourcExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsThereAnyMoreSteps(string jobId = null)
        {
            return Task.FromResult(false);
        }

        public Task PurgeContents(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public Task<TItem> GetNextStep(string jobId = null)
        {
            return Task.FromResult(default(TItem));
        }

        public Task<IEnumerable<TItem>> GetNextStepBatch(int maxBatchSize, string jobId = null)
        {
            return Task.FromResult(Enumerable.Empty<TItem>());
        }

        #region Obsolete members

        public Task EnsureJobQueueExists(string jobId = null)
        {
            return EnsureJobSourcExists(jobId);
        }

        public Task PurgeQueueContents(string jobId = null)
        {
            return PurgeContents(jobId);
        }

        public Task<TItem> Dequeue(string jobId = null)
        {
            return GetNextStep(jobId);
        }

        public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
        {
            return GetNextStepBatch(maxBatchSize, jobId);
        }

        #endregion
    }
}