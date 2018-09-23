using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Connection;
using Nebula.Storage;
using ServiceStack;

namespace Nebula.Queue.Implementation
{
    [Component]
    [Contract]
    public class DelayedJobQueue<TItem> : IDelayedJobQueue<TItem> where TItem : IJobStep
    {
        [ComponentPlug]
        public IRedisManager RedisManager { get; set; }

        public Task EnsureJobSourceExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> Any(string jobId = null)
        {
            var queueLength = await RedisManager.GetDatabase().SortedSetLengthAsync(GetRedisKey(jobId));
            return await Task.FromResult(queueLength > 0);
        }

        public async Task Purge(string jobId = null)
        {
            await RedisManager.GetDatabase().KeyDeleteAsync(GetRedisKey(jobId));
        }

        public async Task<TItem> GetNext(string jobId = null)
        {
            var now = DateTime.UtcNow.Ticks;
            string serialized = await RedisManager.GetDatabase().Pop(GetRedisKey(jobId), now);
            return serialized.FromJson<TItem>();
        }

        public async Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize, string jobId = null)
        {
            var now = DateTime.UtcNow.Ticks;

            var redisKey = GetRedisKey(jobId);
            var redisDb = RedisManager.GetDatabase();
            var tasks = Enumerable
                .Range(1, maxBatchSize)
                .Select(i => redisDb.Pop(redisKey, now));

            var results = await Task.WhenAll(tasks);
            return results
                .Select(r => (string) r)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.FromJson<TItem>());
        }

        public async Task Enqueue(TItem item, long ticks, string jobId = null)
        {
            await RedisManager.GetDatabase().SortedSetAddAsync(GetRedisKey(jobId), item.ToJson(), ticks);
        }

        public async Task EnqueueBatch(IEnumerable<KeyValuePair<TItem, long>> items, string jobId = null)
        {
            var redisKey = GetRedisKey(jobId);
            var redisDb = RedisManager.GetDatabase();
            var tasks = items.Select(item => redisDb.SortedSetAddAsync(redisKey, item.Key.ToJson(), item.Value));
            await Task.WhenAll(tasks);
        }

        private string GetRedisKey(string jobId)
        {
            return "job_" + (string.IsNullOrEmpty(jobId) ? typeof(TItem).Name : jobId);
        }
    }
}