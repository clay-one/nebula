using System;
using ComposerCore.Attributes;
using Nebula.Connection;
using StackExchange.Redis;

namespace Test.Mock
{
    [Component]
    public class MockRedisManager : IRedisManager
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        public string ConnectionString = "localhost:6379";

        public IDatabase GetDatabase()
        {
            if (_connectionMultiplexer == null)
            {
                var options = ConfigurationOptions.Parse(ConnectionString);
                _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            }

            return _connectionMultiplexer.GetDatabase();
        }

        public ISubscriber GetSubscriber()
        {
            throw new NotImplementedException();
        }
    }
}