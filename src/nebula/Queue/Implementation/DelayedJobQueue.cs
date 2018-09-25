﻿using System;
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
        public IRedisConnectionManager RedisManager { get; set; }

        public Task EnsureJobSourceExists(string jobId = null)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> Any(string jobId = null)
        {
            var queueLength = await RedisManager.GetDatabase().SortedSetLengthAsync(GetRedisKey(jobId));
            return queueLength > 0;
        }

        public async Task Purge(string jobId = null)
        {
            await RedisManager.GetDatabase().KeyDeleteAsync(GetRedisKey(jobId));
        }

        public async Task<TItem> GetNext(string jobId = null)
        {
            var now = DateTime.UtcNow.Ticks;
            string serialized = await RedisManager.GetDatabase().SortedSetPopAsync(GetRedisKey(jobId), now);
            return serialized.FromJson<TItem>();
        }

        public async Task<IEnumerable<TItem>> GetNextBatch(int maxBatchSize, string jobId = null)
        {
            var now = DateTime.UtcNow.Ticks;

            var redisKey = GetRedisKey(jobId);
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

        public async Task EnqueueBatch(IEnumerable<Tuple<TItem, DateTime>> items, string jobId = null)
        {
            var redisKey = GetRedisKey(jobId);
            var redisDb = RedisManager.GetDatabase();
            var tasks = items.Select(item =>
                redisDb.SortedSetAddAsync(redisKey, item.Item1.ToJson(), item.Item2.Ticks));
            await Task.WhenAll(tasks);
        }

        public async Task EnqueueBatch(IEnumerable<Tuple<TItem, TimeSpan>> items, string jobId = null)
        {
            var now = DateTime.UtcNow;
            var steps = items.Select(item => new Tuple<TItem, DateTime>(item.Item1, now + item.Item2)).ToList();
            await EnqueueBatch(steps, jobId);
        }

        private string GetRedisKey(string jobId)
        {
            return "job_" + (string.IsNullOrEmpty(jobId) ? typeof(TItem).Name : jobId);
        }
    }
}