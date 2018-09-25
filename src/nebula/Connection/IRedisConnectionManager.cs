using ComposerCore.Attributes;
using StackExchange.Redis;

namespace Nebula.Connection
{
    [Contract]
    public interface IRedisConnectionManager
    {
        IDatabase GetDatabase();
        ISubscriber GetSubscriber();
    }
}