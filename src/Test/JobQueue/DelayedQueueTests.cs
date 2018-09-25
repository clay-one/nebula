using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Connection;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Test.SampleJob.FirstJob;

namespace Test.JobQueue
{
    [TestClass]
    public class DelayedQueueTests : TestClassBase
    {
        private readonly string _jobId = Guid.NewGuid().ToString();

        protected override void ConfigureNebula()
        {
            RegisterMockJobStore();
            RegisterMockBackgroundTaskScheduler();
            RegisterMockRedisManager();

            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);
        }

        [TestMethod]
        public async Task DelayedQueue_Any_Add1ItemAndConsume_ShouldBeFalse()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow, _jobId);
            await queue.GetNext(_jobId);

            Assert.IsFalse(await queue.Any(_jobId));
        }

        [TestMethod]
        public async Task DelayedQueue_Any_Add2ItemAndConsume1_ShouldBeTrue()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 2}, DateTime.UtcNow, _jobId);
            await queue.GetNext(_jobId);

            Assert.IsTrue(await queue.Any(_jobId));
        }

        [TestMethod]
        public async Task DelayedQueue_Purge_Add1ItemAndPurge_ShouldHaveNoMembers()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var jobId = Guid.NewGuid().ToString();
            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow, jobId);
            await queue.Purge(jobId);

            Assert.IsFalse(await queue.Any(jobId));
        }

        [TestMethod]
        public async Task DelayedQueue_GetNext_Add1ItemAndConsume_ShouldBeTheSameAsEnqueuedItem()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var enQueuedItem = new FirstJobStep {Number = 1};
            await queue.Enqueue(enQueuedItem, DateTime.UtcNow, _jobId);
            var dequeuedItem = await queue.GetNext(_jobId);

            Assert.IsNotNull(dequeuedItem);
            Assert.AreEqual(enQueuedItem.Number, dequeuedItem.Number);
        }

        [TestMethod]
        public async Task DelayedQueue_GetNext_Add2ItemAndConsume_DequeuedItemsShouldNotBeTheSame()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 2}, DateTime.UtcNow, _jobId);

            var item1 = await queue.GetNext(_jobId);
            var item2 = await queue.GetNext(_jobId);

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.AreNotEqual(item1.Number, item2.Number);
        }

        [TestMethod]
        public async Task DelayedQueue_GetNextBatch_Add5ItemAndConsume2_ShouldReturn2Items()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            await queue.Enqueue(new FirstJobStep {Number = 1}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 2}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 3}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 4}, DateTime.UtcNow, _jobId);
            await queue.Enqueue(new FirstJobStep {Number = 5}, DateTime.UtcNow, _jobId);
            var items = await queue.GetNextBatch(2, _jobId);

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_EnqueueBatch_Add5ItemAndConsume_ShouldReturn5Items()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var time = DateTime.UtcNow;
            var items = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2},
                new FirstJobStep {Number = 3},
                new FirstJobStep {Number = 4},
                new FirstJobStep {Number = 5}
            };

            await queue.EnqueueBatch(items, time, _jobId);

            var result = await queue.GetNextBatch(5, _jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDifferentTimes_ShouldReturn1()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var time = DateTime.UtcNow;
            var items = new List<Tuple<FirstJobStep, DateTime>>
            {
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep {Number = 1}, time.AddDays(1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep {Number = 2}, time.AddDays(1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep {Number = 3}, time.AddDays(2)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep {Number = 4}, time.AddHours(-1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep {Number = 5}, time.AddHours(1))
            };

            await queue.EnqueueBatch(items, _jobId);

            var result = await queue.GetNextBatch(2, _jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.SingleOrDefault().Number);
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDelay_ShouldReturn1()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var now = DateTime.UtcNow;
            var items = new List<Tuple<FirstJobStep, TimeSpan>>
            {
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep {Number = 1}, now.AddDays(1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep {Number = 2}, now.AddDays(1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep {Number = 3}, now.AddDays(2) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep {Number = 4}, now.AddHours(-1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep {Number = 5}, now.AddHours(1) - now)
            };

            await queue.EnqueueBatch(items, _jobId);

            var result = await queue.GetNextBatch(2, _jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.SingleOrDefault().Number);
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDelay_ShouldReturn0()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var now = DateTime.UtcNow;
            
            var items = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2},
                new FirstJobStep {Number = 3},
                new FirstJobStep {Number = 4},
                new FirstJobStep {Number = 5}
            };

            await queue.EnqueueBatch(items, now.AddHours(1) - now, _jobId);

            var result = await queue.GetNextBatch(2, _jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_Add1ItemWithDelay_ShouldReturn0()
        {
            var queue = Nebula.GetDelayedJobQueue<FirstJobStep>(QueueType.Delayed);

            var now = DateTime.UtcNow;

            await queue.Enqueue(new FirstJobStep { Number = 1 }, now.AddHours(1) - now, _jobId);

            var result = await queue.GetNextBatch(2, _jobId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            var redisManager = Nebula.ComponentContext.GetComponent<IRedisConnectionManager>();
            if (redisManager != null)
                await redisManager.GetDatabase().KeyDeleteAsync("job_" + _jobId);
        }
    }
}