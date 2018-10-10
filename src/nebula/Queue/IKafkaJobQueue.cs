using System.Collections.Generic;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    public interface IKafkaJobQueue<TItem> : IJobStepSource<TItem> where TItem : IJobStep
    {
        void Enqueue(KeyValuePair<string, TItem> item, string jobId = null);
    }
}