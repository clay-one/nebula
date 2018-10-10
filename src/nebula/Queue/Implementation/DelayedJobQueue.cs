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
        private string _jobId;

        [ComponentPlug]
        public IRedisConnectionManager RedisManager { get; set; }

        public void Initialize(string jobId = null)
        {
            _jobId = jobId;
        }

        public Task EnsureJobSourceExists()
        {
            return Task.CompletedTask;
        }

        public async Task<bool> Any()
        {
            var queueLength = await RedisManager.GetDatabase().SortedSetLengthAsync(GetRedisKey());
            return queueLength > 0;
        }

        public async Task Purge()
        {
            await RedisManager.GetDatabase().KeyDeleteAsync(GetRedisKey());
        }

        public async Task<TItem> GetNext()
        {
            var now = DateTime.UtcNow.Ticks;
            string serialized = await RedisManager.GetDatabase().SortedSetPopAsync(GetRedisKey(), now);
            return serialized.FromJson<TItem>();
        }

        public async Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize)
        {
            var now = DateTime.UtcNow.Ticks;

            var redisKey = GetRedisKey();
            var redisDb = RedisManager.GetDatabase();
            var tasks = Enumerable
                .Range(1, maxBatchSize)
                .Select(i => redisDb.SortedSetPopAsync(redisKey, now));

            var results = await Task.WhenAll(tasks);
            return results
                .Select(r => (string) r)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.FromJson<TItem>());
        }

        public async Task EnqueueBatch(IEnumerable<Tuple<TItem, DateTime>> items)
        {
            var redisKey = GetRedisKey();
            var redisDb = RedisManager.GetDatabase();
            var tasks = items.Select(item =>
                redisDb.SortedSetAddAsync(redisKey, item.Item1.ToJson(), item.Item2.Ticks));
            await Task.WhenAll(tasks);
        }

        public async Task EnqueueBatch(IEnumerable<Tuple<TItem, TimeSpan>> items)
        {
            var now = DateTime.UtcNow;
            var steps = items.Select(item => new Tuple<TItem, DateTime>(item.Item1, now + item.Item2)).ToList();
            await EnqueueBatch(steps);
        }

        private string GetRedisKey()
        {
            return "job_" + (string.IsNullOrEmpty(_jobId) ? typeof(TItem).Name : _jobId);
        }
    }
}