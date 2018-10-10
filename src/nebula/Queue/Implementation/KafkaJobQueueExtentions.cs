using System.Collections.Generic;

namespace Nebula.Queue.Implementation
{
    public static class KafkaJobQueueExtentions
    {
        public static void EnqueueBatch<TItem>(this IKafkaJobQueue<TItem> kafkaJobQueue,
            IEnumerable<KeyValuePair<string, TItem>> items, string jobId = null) where TItem : IJobStep
        {
            foreach (var item in items)
                kafkaJobQueue.Enqueue(item, jobId);
        }
    }
}