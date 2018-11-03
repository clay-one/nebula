using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Test.SampleJob.FirstJob;

namespace Test.JobQueue
{
    [TestClass]
    [TestCategory("localTest")]
    public class KafkaJobQueueTests : TestClassBase
    {
        private readonly string _jobId = Guid.NewGuid().ToString();

        protected override void ConfigureNebula()
        {
            Nebula.KafkaConfig = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("bootstrap.servers", "172.30.3.59:9101"),
                new KeyValuePair<string, object>("group.id", "testGroup"),
                new KeyValuePair<string, object>("auto.offset.reset", "earliest"),
                new KeyValuePair<string, object>("queue.buffering.max.ms", 1),
                new KeyValuePair<string, object>("batch.num.messages", 1),
                new KeyValuePair<string, object>("fetch.wait.max.ms", 5000)
            };

            Nebula.RegisterJobQueue(typeof(KafkaJobQueue<>), QueueType.Kafka);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Enqueue_Enqueue1ItemandConsume()
        {
            var itemToEnqueue = new FirstJobStep {Number = 10};

            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(itemToEnqueue);
            FirstJobStep item = null;

            for (var i = 0; i < 600; i++)
            {
                item = await queue.GetNext();
                if (item != null)
                    break;
            }

            Assert.IsNotNull(item);
            Assert.AreEqual(itemToEnqueue.Number, item.Number);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Commit_ShouldNotReturnSameObject()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(new FirstJobStep {Number = 1});
            queue.Enqueue(new FirstJobStep {Number = 2});
            queue.Enqueue(new FirstJobStep {Number = 3});

            FirstJobStep item1 = null, item2 = null;

            for (var i = 0; i < 600; i++)
            {
                item1 = await queue.GetNext();
                if (item1 != null)
                    break;
            }

            for (var i = 0; i < 600; i++)
            {
                item2 = await queue.GetNext();
                if (item2 != null)
                    break;
            }

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.AreNotEqual(item1.Number, item2.Number);
        }

        [TestMethod]
        public async Task KafkaJobQueue_GetNextBatch_Enqueue5Get2()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(new FirstJobStep {Number = 1});
            queue.Enqueue(new FirstJobStep {Number = 2});
            queue.Enqueue(new FirstJobStep {Number = 3});
            queue.Enqueue(new FirstJobStep {Number = 4});
            queue.Enqueue(new FirstJobStep {Number = 5});

            for (var i = 0; i < 600; i++)
            {
                var item1 = await queue.GetNext();
                if (item1 != null)
                    break;
            }

            List<FirstJobStep> items = null;

            for (var i = 0; i < 600; i++)
            {
                items = (await queue.GetNextBatch(2)).ToList();

                if (items.Any())
                    break;
            }

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task KafkaJobQueue_EnqueueBatch_Enqueue2Get2()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2},
                new FirstJobStep {Number = 3},
                new FirstJobStep {Number = 4},
                new FirstJobStep {Number = 5}
            };

            queue.EnqueueBatch(steps);

            for (var i = 0; i < 600; i++)
            {
                var item1 = await queue.GetNext();
                if (item1 != null)
                    break;
            }

            List<FirstJobStep> items = null;
            for (var i = 0; i < 600; i++)
            {
                items = (await queue.GetNextBatch(2)).ToList();

                if (items.Any())
                    break;
            }

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task KafkaJobQueue_Purge_ShouldReturnNull()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2}
            };

            queue.EnqueueBatch(steps);

            for (var i = 0; i < 600; i++)
                if ((await queue.GetNextBatch(2)).Any())
                    break;

            await queue.Purge();

            var items = new List<FirstJobStep>();

            for (var i = 0; i < 600; i++)
            {
                items = (List<FirstJobStep>) await queue.GetNextBatch(2);

                if (items.Any())
                    break;
            }

            Assert.IsFalse(items.Any());
        }

        [TestMethod]
        public async Task KafkaJobQueue_Any_EnqueueConsumeShouldReturnTrue()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2},
                new FirstJobStep {Number = 3},
                new FirstJobStep {Number = 4},
                new FirstJobStep {Number = 5}
            };

            queue.EnqueueBatch(steps);

            for (var i = 0; i < 600; i++)
                if ((await queue.GetNextBatch(2)).Any())
                    break;

            var queueHasItems = await queue.Any();

            Assert.IsTrue(queueHasItems);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Any_EnqueueShouldReturnTrue()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<FirstJobStep>
            {
                new FirstJobStep {Number = 1},
                new FirstJobStep {Number = 2},
                new FirstJobStep {Number = 3},
                new FirstJobStep {Number = 4},
                new FirstJobStep {Number = 5}
            };

            queue.EnqueueBatch(steps);

            var queueHasItems = await queue.Any();

            Assert.IsTrue(queueHasItems);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Any_EmptyQueueReturnFalse()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var queueHasItems = await queue.Any();

            Assert.IsFalse(queueHasItems);
        }
    }
}