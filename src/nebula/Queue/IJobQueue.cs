using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    public interface IJobQueue
    {
        Task<long> GetQueueLength();
    }

    [Contract]
    public interface IJobQueue<TItem> : IJobStepSource<TItem>, IJobQueue where TItem : IJobStep
    {
        Task Enqueue(TItem item);
        Task EnqueueBatch(IEnumerable<TItem> items);
    }
}