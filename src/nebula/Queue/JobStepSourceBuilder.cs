using ComposerCore;
using ComposerCore.Attributes;
using ComposerCore.Factories;

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
            switch (queueTypeName)
            {
                case QueueType.Delayed:
                    return BuildDelayedJobQueue<TJobStep>(jobId);
                case QueueType.InMemory:
                    return BuildInMemoryJobQueue<TJobStep>(jobId);
                case QueueType.Inline:
                    return BuildInlineJobQueue<TJobStep>(jobId);
                case QueueType.Kafka:
                    return BuildKafkaJobQueue<TJobStep>(jobId);
                case QueueType.Redis:
                    return BuildRedisJobQueue<TJobStep>(jobId);
                default:
                    return BuildJobStepSourceInternal<TJobStep>(queueTypeName, jobId);
            }
        }

        public IJobQueue<TJobStep> BuildRedisJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSourceInternal<TJobStep>(QueueType.Redis, GetRedisKey<TJobStep>(jobId)) as
                IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInlineJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSourceInternal<TJobStep>(QueueType.Inline, jobId) as IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInMemoryJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSourceInternal<TJobStep>(QueueType.InMemory, jobId) as IJobQueue<TJobStep>;
        }

        public IDelayedJobQueue<TJobStep> BuildDelayedJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSourceInternal<TJobStep>(QueueType.Delayed, GetRedisKey<TJobStep>(jobId)) as
                IDelayedJobQueue<TJobStep>;
        }

        public IKafkaJobQueue<TJobStep> BuildKafkaJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSourceInternal<TJobStep>(QueueType.Kafka, GetKafkaTopic<TJobStep>(jobId)) as
                IKafkaJobQueue<TJobStep>;
        }

        private IJobStepSource<TJobStep> BuildJobStepSourceInternal<TJobStep>(string queueTypeName, string jobId)
            where TJobStep : IJobStep
        {
            IJobStepSource<TJobStep> jobStepSource;

            if (!string.IsNullOrEmpty(jobId))
            {
                jobStepSource = ComponentContext.GetComponent<IJobStepSource<TJobStep>>(jobId);

                if (jobStepSource != null)
                    return jobStepSource;
            }

            jobStepSource = ComponentContext.GetComponent<IJobStepSource<TJobStep>>(queueTypeName);
            if (jobStepSource == null)
                return null;

            jobStepSource.Initialize(jobId);
            ComponentContext.Register(typeof(IJobStepSource<>), jobId,
                new PreInitializedComponentFactory(jobStepSource));

            return jobStepSource;
        }

        private string GetKafkaTopic<TJobStep>(string jobId)
        {
            return "job_" + (string.IsNullOrEmpty(jobId) ? typeof(TJobStep).Name : jobId);
        }

        private string GetRedisKey<TJobStep>(string jobId)
        {
            return "job_" + (string.IsNullOrEmpty(jobId) ? typeof(TJobStep).Name : jobId);
        }
    }
}