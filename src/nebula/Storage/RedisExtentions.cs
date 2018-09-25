using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Nebula.Storage
{
    public static class RedisExtentions
    {
        public static async Task<RedisValue> SortedSetPopAsync(this IDatabase db, RedisKey key, double maxScore)
        {
            try
            {
                var script =
                    @"local result = redis.call('ZRANGEBYSCORE', @key, '-inf', @maxScore, 'WITHSCORES' , 'LIMIT', '0', '1' )
                               local member = result[1]
                               if member then
                                   redis.call('ZREM', @key, member)
                                   return member
                               else
                                   return nil
                               end";

                var prepared = LuaScript.Prepare(script);
                var result = await db.ScriptEvaluateAsync(prepared, new {key, maxScore});

                return result.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}