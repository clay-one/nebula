using System.Collections.Generic;
using System.Threading.Tasks;
using Appson.JobQueue.Queue;
using Appson.JobQueue.Storage.Model;

namespace SampleJob
{
    public class SampleJobProcessor : IJobProcessor<SampleJobStep>
    {
        public void Initialize(JobData jobData)
        {
        }

        public Task<JobProcessingResult> Process(List<SampleJobStep> items)
        {
            return null;
        }
        
        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }
}