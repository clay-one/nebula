using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    public interface IDelayedJobQueue<TItem> : IJobStepSource<TItem> where TItem : IJobStep
    {
        Task EnqueueBatch(IEnumerable<Tuple<TItem, DateTime>> items);
        Task EnqueueBatch(IEnumerable<Tuple<TItem, TimeSpan>> items);
    }
}