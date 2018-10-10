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
            var jobStepSource = GetComponent<TJobStep>(queueTypeName) as IJobStepSource<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        public IJobQueue<TJobStep> BuildRedisJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource = GetComponent<TJobStep>(QueueType.Redis) as IJobQueue<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        public IJobQueue<TJobStep> BuildInlineJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource = GetComponent<TJobStep>(QueueType.Inline) as IJobQueue<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        public IJobQueue<TJobStep> BuildInMemoryJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource = GetComponent<TJobStep>(QueueType.InMemory) as IJobQueue<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        public IDelayedJobQueue<TJobStep> BuildDelayedJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource = GetComponent<TJobStep>(QueueType.Delayed) as IDelayedJobQueue<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        public IKafkaJobQueue<TJobStep> BuildKafkaJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource = GetComponent<TJobStep>(QueueType.Kafka) as IKafkaJobQueue<TJobStep>;
            jobStepSource?.Initialize(jobId);
            return jobStepSource;
        }

        private object GetComponent<TJobStep>(string queueTypeName) where TJobStep : IJobStep
        {
            return ComponentContext.GetComponent(typeof(IJobStepSource<TJobStep>), queueTypeName);
        }
    }
}