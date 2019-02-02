using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using log4net;
using ServiceStack;

namespace Nebula.Queue.Implementation
{
    [Component]
    [ComponentCache(null)]
    public class KafkaJobQueue<TItem> : IKafkaJobQueue<TItem> where TItem : IJobStep
    {
        private Consumer<Null, string> _consumer;
        private Producer<Null, string> _producer;
        private string _topic;

        protected static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [ComponentPlug]
        public NebulaContext NebulaContext { get; set; }

        public void Initialize(string jobId )
        {
            _topic = jobId;

            if(_consumer!=null && _producer!=null)
                return;

             _producer = new Producer<Null, string>(NebulaContext.KafkaConfig, null,
                new StringSerializer(Encoding.UTF8));
            _consumer =
                new Consumer<Null, string>(NebulaContext.KafkaConfig, null, new StringDeserializer(Encoding.UTF8));
            _consumer.Subscribe(_topic);

            _producer.OnError += ProducerOnOnError;
            _consumer.OnError += ConsumerOnOnError;
            _consumer.OnConsumeError += _consumer_OnConsumeError;
        }
        
        public void Enqueue(TItem item)
        {
            var value = item.ToJson();
            _producer.ProduceAsync(_topic, null, value, 0).GetAwaiter().GetResult();
        }

        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public async Task<bool> Any()
        {
            var positions = _consumer.Position(new List<TopicPartition> {new TopicPartition(_topic, 0)});

            return await Task.FromResult(positions[0].Offset.Value > 0);
        }

        public async Task Purge()
        {
            var metadata = _consumer.GetMetadata(false);
            var partitions = metadata.Topics.Single(a => a.Topic == _topic).Partitions;

            foreach (var partition in partitions)
                _consumer.Assign(new List<TopicPartitionOffset>
                {
                    new TopicPartitionOffset(_topic, partition.PartitionId, Offset.End)
                });
            await _consumer.CommitAsync();
        }

        public async Task<TItem> GetNext()
        {
            Message<Null, string> message = null;

            if (!_consumer.Consume(out message, TimeSpan.FromMilliseconds(100)))
                return default(TItem);

            return await Task.FromResult(message.Value.FromJson<TItem>());
        }

        public async Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            var tasks = Enumerable
                .Range(1, maxBatchSize)
                .Select(i => GetNext());

            var results = await Task.WhenAll(tasks);

            return results.Where(a => a != null).ToList();
        }

        private void ProducerOnOnError(object sender, Error error)
        {
            Log.Error($"Kafka producer could not produce due to error: {error}");
        }

        private void ConsumerOnOnError(object sender, Error error)
        {
            Log.Error($"Kafka cunsumer could not consume due to error: {error}");
        }

        private void _consumer_OnConsumeError(object sender, Message e)
        {
            Log.Error($"Kafka cunsumer could not consume due to error: {e}");
        }

        ~KafkaJobQueue()
        {
            _producer?.Dispose();
            _consumer?.Dispose();
        }
    }
}