using ComposerCore;
using ComposerCore.Attributes;

namespace Nebula.Queue
{
    [Contract]
    [Component]
    public class JobStepSourceBuilder
    {
        [ComponentPlug]
        public IComponentContext ComponentContext { get; set; }

        public IJobStepSource<TJobStep> BuildJobStepSource<TJobStep>(string queueTypeName, string jobId = null)
            where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(queueTypeName) as IJobStepSource<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildRedisJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(QueueType.Redis) as IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInlineJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(QueueType.Inline) as IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInMemoryJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(QueueType.InMemory) as IJobQueue<TJobStep>;
        }

        public IDelayedJobQueue<TJobStep> BuildDelayedJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(QueueType.Delayed) as IDelayedJobQueue<TJobStep>;
        }

        public IKafkaJobQueue<TJobStep> BuildKafkaJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return GetComponent<TJobStep>(QueueType.Kafka) as IKafkaJobQueue<TJobStep>;
        }

        private object GetComponent<TJobStep>(string queueTypeName) where TJobStep : IJobStep
        {
            return ComponentContext.GetComponent(typeof(IJobStepSource<TJobStep>), queueTypeName);
        }
    }
}