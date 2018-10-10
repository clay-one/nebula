using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using ServiceStack;

namespace Nebula.Queue.Implementation
{
    [Component]
    public class KafkaJobQueue<TItem> : IKafkaJobQueue<TItem> where TItem : IJobStep
    {
        private Consumer<Null, string> _consumer;
        private string _jobId;
        private Producer<Null, string> _producer;

        [ComponentPlug]
        public NebulaContext NebulaContext { get; set; }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
        }

        public void Enqueue(KeyValuePair<string, TItem> item)
        {
            var topic = GetTopic();

            var value = item.Value.ToJson();
            _producer.ProduceAsync(topic, null, value);
        }
        
        public Task EnsureJobSourceExists()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Any()
        {
            throw new NotImplementedException();
        }

        public async Task Purge()
        {
            _consumer.Subscribe(GetTopic());

            var metadata = _consumer.GetMetadata(false);
            var partitions = metadata.Topics.Single(a => a.Topic == GetTopic()).Partitions;

            foreach (var partition in partitions)
                _consumer.Assign(new List<TopicPartitionOffset>
                {
                    new TopicPartitionOffset(GetTopic(), partition.PartitionId, Offset.End)
                });
            await _consumer.CommitAsync();
        }

        public async Task<TItem> GetNext()
        {
            _consumer.Subscribe(GetTopic());

            _consumer.OnError += ConsumerOnOnError;
            Message<Null, string> message = null;

            if (!_consumer.Consume(out message, TimeSpan.FromMilliseconds(100)))
                return default(TItem);

            await _consumer.CommitAsync();
            return message.Value.FromJson<TItem>();

            //for (var i = 0; i < 20; i++)
            //    if (_consumer.Consume(out message, TimeSpan.FromMilliseconds(100)))
            //        break;

            //if (message == null)
            //    return default(TItem);

            //await _consumer.CommitAsync();
            //return message.Value.FromJson<TItem>();
        }

        public async Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            var tasks = Enumerable
                .Range(1, maxBatchSize)
                .Select(i => GetNext());

            var results = await Task.WhenAll(tasks);

            return results.Where(a => a != null).ToList();
        }

        [OnCompositionComplete]
        public void OnCompositionComplete()
        {
            _producer = new Producer<Null, string>(NebulaContext.KafkaConfig, null,
                new StringSerializer(Encoding.UTF8));
            _consumer =
                new Consumer<Null, string>(NebulaContext.KafkaConfig, null, new StringDeserializer(Encoding.UTF8));

            _producer.OnError += ProducerOnOnError;
        }

        private void ProducerOnOnError(object sender, Error error)
        {
        }

        private void ConsumerOnOnError(object sender, Error error)
        {
        }

        private string GetTopic()
        {
            return "job_" + (string.IsNullOrEmpty(_jobId) ? typeof(TItem).Name : _jobId);
        }

        ~KafkaJobQueue()
        {
            _producer?.Dispose();
            _consumer?.Dispose();
        }
    }
}