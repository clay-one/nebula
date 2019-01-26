using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace Test.SampleJob.FirstJob
{
    public class FirstJobProcessor : IJobProcessor<FirstJobStep>
    {
        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
        }

        public async Task<JobProcessingResult> Process(List<FirstJobStep> items)
        {
            return await Task.FromResult(new JobProcessingResult());
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }
}