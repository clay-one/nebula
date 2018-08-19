using System.Collections.Generic;
using System.Threading.Tasks;
using Nebula;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace SampleJob
{
    public class SampleJobProcessor : IJobProcessor<SampleJobStep>
    {
        private NebulaContext _nebulaContext;
        private static int _index;
        public void Initialize(JobData jobData,NebulaContext nebulaContext)
        {
            _nebulaContext = nebulaContext;
        }

        public async Task<JobProcessingResult> Process(List<SampleJobStep> items)
        {
            var initialStep = new SampleJobStep
            {
                Number = 10000
            };

            var queue = _nebulaContext.GetJobQueue<SampleJobStep>(QueueType.Redis);
            await queue.Enqueue(initialStep, "sample-job");

            _index++;
           return new JobProcessingResult();
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }
}