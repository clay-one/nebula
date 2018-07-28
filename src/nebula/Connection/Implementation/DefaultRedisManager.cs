using ComposerCore.Attributes;
using StackExchange.Redis;

namespace Nebula.Connection.Implementation
{
    [Component]
    internal class DefaultRedisManager : IRedisManager
    {
        private ConnectionMultiplexer _connectionMultiplexer;

        [ConfigurationPoint("redis.configurationString")]
        public string ConfigurationString { get; set; }

        public IDatabase GetDatabase()
        {
            return _connectionMultiplexer.GetDatabase();
        }

        public ISubscriber GetSubscriber()
        {
            return _connectionMultiplexer.GetSubscriber();
        }

        [OnCompositionComplete]
        public void OnCompositionComplete()
        {
            var options = ConfigurationOptions.Parse(ConfigurationString);
            _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        }
    }
}