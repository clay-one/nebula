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
    public class KafkaJobQueueTests : TestClassBase
    {
        private readonly string _jobId = Guid.NewGuid().ToString();

        protected override void ConfigureNebula()
        {
            Nebula.KafkaConfig = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("bootstrap.servers", "172.30.3.59:9101"),
                new KeyValuePair<string, object>("group.id", "testGroup"),
                new KeyValuePair<string, object>("auto.offset.reset", "earliest")
            };

            Nebula.RegisterJobQueue(typeof(KafkaJobQueue<>), QueueType.Kafka);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Enqueue_Enqueue1ItemandConsume()
        {
            var itemToEnqueue = new FirstJobStep {Number = 10};

            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", itemToEnqueue));
            FirstJobStep item = null;

            for (int i = 0; i < 10; i++)
            {
                item = await queue.GetNext();
                if(item!=null)
                    break;
            }

            Assert.IsNotNull(item);
            Assert.AreEqual(itemToEnqueue.Number, item.Number);
        }

        [TestMethod]
        public async Task KafkaJobQueue_Commit_ShouldNotReturnSameObject()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 1}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 2}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 3}));

            var item1 = await queue.GetNext();
            var item2 = await queue.GetNext();

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);
            Assert.AreNotEqual(item1.Number, item2.Number);
        }

        [TestMethod]
        public async Task KafkaJobQueue_GetNextBatch_Enqueue5Get2()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 1}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 2}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 3}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 4}));
            queue.Enqueue(new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 5}));

            var items = await queue.GetNextBatch(2);

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task KafkaJobQueue_EnqueueBatch_Enqueue2Get2()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<KeyValuePair<string, FirstJobStep>>
            {
                new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 1}),
                new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 2})
            };

            queue.EnqueueBatch(steps);

            var items = await queue.GetNextBatch(2);

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count());
        }

        [TestMethod]
        public async Task KafkaJobQueue_Purge_ShouldReturnNull()
        {
            var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<FirstJobStep>(_jobId);

            var steps = new List<KeyValuePair<string, FirstJobStep>>
            {
                new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 1}),
                new KeyValuePair<string, FirstJobStep>("sampleKey", new FirstJobStep {Number = 2})
            };

            queue.EnqueueBatch(steps);

            await queue.Purge();

            var items = await queue.GetNextBatch(2);

            Assert.IsFalse(items.Any());
        }
    }
}