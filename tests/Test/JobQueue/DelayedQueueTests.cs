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
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            await queue.Enqueue(new FirstJobStep { Number = 1 }, DateTime.UtcNow);
            await queue.GetNext();

            Assert.IsFalse(await queue.Any());
        }

        [TestMethod]
        public async Task DelayedQueue_Any_Add2ItemAndConsume1_ShouldBeTrue()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            await queue.Enqueue(new FirstJobStep { Number = 1 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 2 }, DateTime.UtcNow);
            await queue.GetNext();

            Assert.IsTrue(await queue.Any());
        }

        [TestMethod]
        public async Task DelayedQueue_Purge_Add1ItemAndPurge_ShouldHaveNoMembers()
        {
            var jobId = Guid.NewGuid().ToString();
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(jobId);

            await queue.Enqueue(new FirstJobStep { Number = 1 }, DateTime.UtcNow);
            await queue.Purge();

            Assert.IsFalse(await queue.Any());
        }

        [TestMethod]
        public async Task DelayedQueue_GetNext_Add1ItemAndConsume_ShouldBeTheSameAsEnqueuedItem()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var enQueuedItem = new FirstJobStep { Number = 1 };
            await queue.Enqueue(enQueuedItem, DateTime.UtcNow);
            var dequeuedItem = await queue.GetNext();

            Assert.IsNotNull(dequeuedItem);
            Assert.AreEqual(enQueuedItem.Number, dequeuedItem.Number);
        }

        [TestMethod]
        public async Task DelayedQueue_GetNext_Add2ItemAndConsume_DequeuedItemsShouldNotBeTheSame()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            await queue.Enqueue(new FirstJobStep { Number = 1 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 2 }, DateTime.UtcNow);

            var item1 = await queue.GetNext();
            var item2 = await queue.GetNext();

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.AreNotEqual(item1.Number, item2.Number);
        }

        [TestMethod]
        public async Task DelayedQueue_GetNextBatch_Add5ItemAndConsume2_ShouldReturn2Items()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            await queue.Enqueue(new FirstJobStep { Number = 1 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 2 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 3 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 4 }, DateTime.UtcNow);
            await queue.Enqueue(new FirstJobStep { Number = 5 }, DateTime.UtcNow);
            var items = await queue.GetNextBatch(2);

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_EnqueueBatch_Add5ItemAndConsume_ShouldReturn5Items()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var time = DateTime.UtcNow;
            var items = new List<FirstJobStep>
            {
                new FirstJobStep { Number = 1 },
                new FirstJobStep { Number = 2 },
                new FirstJobStep { Number = 3 },
                new FirstJobStep { Number = 4 },
                new FirstJobStep { Number = 5 }
            };

            await queue.EnqueueBatch(items, time);

            var result = await queue.GetNextBatch(5);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDifferentTimes_ShouldReturn1()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var time = DateTime.UtcNow;
            var items = new List<Tuple<FirstJobStep, DateTime>>
            {
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep { Number = 1 }, time.AddDays(1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep { Number = 2 }, time.AddDays(1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep { Number = 3 }, time.AddDays(2)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep { Number = 4 }, time.AddHours(-1)),
                new Tuple<FirstJobStep, DateTime>(new FirstJobStep { Number = 5 }, time.AddHours(1))
            };

            await queue.EnqueueBatch(items);

            var result = await queue.GetNextBatch(2);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.SingleOrDefault().Number);
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDelay_ShouldReturn1()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var now = DateTime.UtcNow;
            var items = new List<Tuple<FirstJobStep, TimeSpan>>
            {
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep { Number = 1 }, now.AddDays(1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep { Number = 2 }, now.AddDays(1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep { Number = 3 }, now.AddDays(2) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep { Number = 4 }, now.AddHours(-1) - now),
                new Tuple<FirstJobStep, TimeSpan>(new FirstJobStep { Number = 5 }, now.AddHours(1) - now)
            };

            await queue.EnqueueBatch(items);

            var result = await queue.GetNextBatch(2);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(4, result.SingleOrDefault().Number);
        }

        [TestMethod]
        public async Task DelayedQueue_Add5ItemWithDelay_ShouldReturn0()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var now = DateTime.UtcNow;

            var items = new List<FirstJobStep>
            {
                new FirstJobStep { Number = 1 },
                new FirstJobStep { Number = 2 },
                new FirstJobStep { Number = 3 },
                new FirstJobStep { Number = 4 },
                new FirstJobStep { Number = 5 }
            };

            await queue.EnqueueBatch(items, now.AddHours(1) - now);

            var result = await queue.GetNextBatch(2);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task DelayedQueue_Add1ItemWithDelay_ShouldReturn0()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(_jobId);

            var now = DateTime.UtcNow;

            await queue.Enqueue(new FirstJobStep { Number = 1 }, now.AddHours(1) - now);

            var result = await queue.GetNextBatch(2);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void Given_Different_Previously_Unknown_JobIds_Should_Return_Different_JobQueue_Instances()
        {
            var (jobId1, jobId2) = (Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));

            var q1 = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(jobId1);
            var q2 = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(jobId2);

            Assert.AreNotEqual(q2, q1);
        }

        [TestMethod]
        public async Task Should_Enqueue_Specific_Items_To_Their_Own_Queue_Instance()
        {
            var (jobId1, jobId2) = (Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));

            var q1 = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(jobId1);
            var q2 = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<FirstJobStep>(jobId2);

            foreach (var i in Enumerable.Range(1, 10))
            {
                await q1.Enqueue(new FirstJobStep { Number = 10 + i }, TimeSpan.Zero);
                await q2.Enqueue(new FirstJobStep { Number = 20 + i }, TimeSpan.Zero);
            }

            var q1Items = (await q1.GetNextBatch(10)).ToList();

            var q1Remaining = (await q1.GetNextBatch(10)).ToList();
            Assert.AreEqual(0, q1Remaining.Count);

            Assert.AreEqual(10, q1Items.Count);
            Assert.IsTrue(q1Items.All(c => c.Number <= 20));
            Assert.IsFalse(q1Items.Any(c => c.Number > 20));


            var q2Items = (await q2.GetNextBatch(10)).ToList();

            var q2Remaining = (await q2.GetNextBatch(10)).ToList();
            Assert.AreEqual(0, q2Remaining.Count);

            Assert.AreEqual(10, q2Items.Count);
            Assert.IsTrue(q2Items.All(c => c.Number >= 20));
            Assert.IsFalse(q2Items.Any(c => c.Number < 20));
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