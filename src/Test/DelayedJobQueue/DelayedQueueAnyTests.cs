using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Connection;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Test.SampleJob.FirstJob;

namespace Test.DelayedJobQueue
{
    [TestClass]
    public class DelayedQueueAnyTests : TestClassBase
    {
        private readonly string _jobId = Guid.NewGuid().ToString();

        protected override void ConfigureNebula()
        {
            RegisterMockJobStore();
            RegisterMockBackgroundTaskScheduler();
            RegisterMockRedisManager();
        }

        [TestMethod]
        public async Task DelayedQueueAny_Add1ItemAndConsume_ShouldBeFalse()
        {
            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var jobId = Guid.NewGuid().ToString();
            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow.Ticks, jobId);
            await queue.GetNext(jobId);

            Assert.IsFalse(await queue.Any(jobId));
        }

        [TestMethod]
        public async Task DelayedQueueAny_Add2ItemAndConsume1_ShouldBeTrue()
        {
            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow.Ticks, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 2}, DateTime.UtcNow.Ticks, _jobId);
            await queue.GetNext(_jobId);

            Assert.IsTrue(await queue.Any(_jobId));
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            var redisManager = Nebula.ComponentContext.GetComponent<IRedisManager>();
            if (redisManager != null)
                await redisManager.GetDatabase().KeyDeleteAsync("job_" + _jobId);
        }
    }
}