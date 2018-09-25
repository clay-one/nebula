using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    public interface IDelayedJobQueue<TItem> : IJobStepSource<TItem> where TItem : IJobStep
    {
        Task Enqueue(TItem item, DateTime processTime, string jobId = null);
        Task EnqueueBatch(IEnumerable<Tuple<TItem, DateTime>> items, string jobId = null);
        Task EnqueueBatch(IEnumerable<TItem> items, DateTime processTime, string jobId = null);
        Task EnqueueBatch(IEnumerable<Tuple<TItem, TimeSpan>> items, string jobId = null);
    }
}