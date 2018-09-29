using ComposerCore.Attributes;
using StackExchange.Redis;

namespace Nebula.Connection.Implementation
{
    [Component]
    internal class DefaultRedisManager : IRedisConnectionManager
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        
        [ComponentPlug]
        public NebulaContext NebulaContext { get; set; }

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
            var options = ConfigurationOptions.Parse(NebulaContext.RedisConnectionString);
            _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        }
    }
}