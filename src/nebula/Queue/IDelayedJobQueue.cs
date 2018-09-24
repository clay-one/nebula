﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    public interface IDelayedJobQueue<TItem> : IJobStepSource<TItem> where TItem : IJobStep
    {
        Task Enqueue(TItem item, long ticks, string jobId = null);
        Task EnqueueBatch(IEnumerable<KeyValuePair<TItem, long>> items, string jobId = null);
    }
}