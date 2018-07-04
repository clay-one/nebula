using ComposerCore.Attributes;
using StackExchange.Redis;

namespace Nebula.Connection
{
    [Contract]
    public interface IRedisManager
    {
        IDatabase GetDatabase();
        ISubscriber GetSubscriber();
    }
}