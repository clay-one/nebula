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

        public Task<long> GetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public async Task Enqueue(TItem item)
        {
            await Processor.Process(item.Yield().ToList());
        }

        public async Task EnqueueBatch(IEnumerable<TItem> items)
        {
            await Processor.Process(items.ToList());
        }

        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public Task<bool> Any()
        {
            return Task.FromResult(false);
        }

        public Task Purge()
        {
            return Task.CompletedTask;
        }

        public Task<TItem> GetNext()
        {
            return Task.FromResult(default(TItem));
        }

        public Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            return Task.FromResult(Enumerable.Empty<TItem>());
        }
    }
}