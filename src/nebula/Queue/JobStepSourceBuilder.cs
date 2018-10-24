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

        public IJobQueue<TJobStep> BuildRedisJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSource<TJobStep>(QueueType.Redis, GetRedisKey<TJobStep>(jobId)) as IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInlineJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSource<TJobStep>(QueueType.Inline, jobId) as IJobQueue<TJobStep>;
        }

        public IJobQueue<TJobStep> BuildInMemoryJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSource<TJobStep>(QueueType.InMemory, jobId) as IJobQueue<TJobStep>;
        }

        public IDelayedJobQueue<TJobStep> BuildDelayedJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            return BuildJobStepSource<TJobStep>(QueueType.Delayed, GetRedisKey<TJobStep>(jobId)) as
                IDelayedJobQueue<TJobStep>;
        }

        public IKafkaJobQueue<TJobStep> BuildKafkaJobQueue<TJobStep>(string jobId = null) where TJobStep : IJobStep
        {
            var jobStepSource =
                BuildJobStepSource<TJobStep>(QueueType.Kafka, GetKafkaTopic<TJobStep>(jobId)) as
                    IKafkaJobQueue<TJobStep>;

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