using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Test.SampleJob;
using Test.SampleJob.FirstJob;
using Test.SampleJob.SecondJob;

namespace Test.JobQueue
{
    [TestClass]
    public class JobStepSourceBuilderTests : TestClassBase
    {
        private readonly string _jobId = "jobId";

        protected override void ConfigureNebula()
        {
            RegisterMockRedisManager();

            Nebula.RegisterJobQueue(typeof(FirstJobQueue<FirstJobStep>), nameof(FirstJobStep));
            Nebula.RegisterJobQueue(typeof(SecondJobQueue<SecondJobStep>), nameof(SecondJobStep));
            Nebula.RegisterJobQueue(typeof(KafkaJobQueue<>), QueueType.Kafka);
            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<>), QueueType.InMemory);
            Nebula.RegisterJobQueue(typeof(RedisJobQueue<>), QueueType.Redis);
            Nebula.RegisterJobQueue(typeof(InlineJobQueue<>), QueueType.Inline);
            Nebula.RegisterJobQueue(typeof(NullJobQueue<>), QueueType.Null);

            Nebula.KafkaConfig = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("bootstrap.servers", "172.30.3.59:9101"),
                new KeyValuePair<string, object>("group.id", "testGroup"),
                new KeyValuePair<string, object>("auto.offset.reset", "earliest"),
                new KeyValuePair<string, object>("queue.buffering.max.ms", 1),
                new KeyValuePair<string, object>("batch.num.messages", 1),
                new KeyValuePair<string, object>("fetch.wait.max.ms", 5000)
            };
        }

        [TestMethod]
        public void JobStepSourceBuilder_DifferentJobIds_DifferentQueues()
        {
            var jobId1 = "jobId1";
            var jobId2 = "jobId2";

            var queue1 = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(jobId1);
            var queue2 = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(jobId2);

            Assert.IsNotNull(queue1);
            Assert.IsNotNull(queue2);
            Assert.AreNotEqual(queue1, queue2);
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetSameJobQueue_ShouldInitializeOnce()
        {
            Nebula.RegisterJobQueue(typeof(FirstJobQueue<>), QueueTypes.FirstJobQueue);

            var jobId1 = "jobId1";

            var queue1 =
                Nebula.JobStepSourceBuilder.BuildJobStepSource<FirstJobStep>(QueueTypes.FirstJobQueue, jobId1) as
                    FirstJobQueue<FirstJobStep>;

            var queue2 =
                Nebula.JobStepSourceBuilder.BuildJobStepSource<FirstJobStep>(QueueTypes.FirstJobQueue, jobId1) as
                    FirstJobQueue<FirstJobStep>;

            Assert.IsNotNull(queue1);
            Assert.IsNotNull(queue2);
            Assert.AreEqual(queue1, queue2);
            Assert.AreEqual(1, queue1.InitializationCount);
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetInMemoryJobQueue_TypeShouldBeCorrect()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildInMemoryJobQueue<FirstJobStep>(_jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(InMemoryJobQueue<FirstJobStep>), queue.GetType());
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetDelayedJobQueue_TypeShouldBeCorrect()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(DelayedJobQueue<FirstJobStep>), queue.GetType());
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetKafkaJobQueue_TypeShouldBeCorrect()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(KafkaJobQueue<FirstJobStep>), queue.GetType());
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetRedisJobQueue_TypeShouldBeCorrect()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildRedisJobQueue<FirstJobStep>(_jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(RedisJobQueue<FirstJobStep>), queue.GetType());
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetInlineJobQueue_TypeShouldBeCorrect()
        {
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor),typeof(FirstJobStep));
            var queue = Nebula.JobStepSourceBuilder.BuildInlineJobQueue<FirstJobStep>(_jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(InlineJobQueue<FirstJobStep>), queue.GetType());
        }

        [TestMethod]
        public void JobStepSourceBuilder_GetNullJobQueue_TypeShouldBeCorrect()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildJobStepSource<FirstJobStep>(QueueType.Null, _jobId);

            Assert.IsNotNull(queue);
            Assert.AreEqual(typeof(NullJobQueue<FirstJobStep>), queue.GetType());
        }
    }
}