using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;
using SampleJob;

namespace SampleWorker
{
    public class SampleJobProcessor : IFinalizableJobProcessor<SampleJobStep>
    {
        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
        }

        public async Task<JobProcessingResult> Process(List<SampleJobStep> items)
        {
            Console.WriteLine($"processing {items.Count} items");
            foreach (var item in items)
            {
                Console.WriteLine($"processing item: {item.Number}");
            }
            return await Task.FromResult(new JobProcessingResult());
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public async Task JobCompleted()
        {
            await Task.CompletedTask;
        }
    }
}