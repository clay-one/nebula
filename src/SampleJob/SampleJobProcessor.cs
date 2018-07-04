using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace SampleJob
{
    [Component]
    public class SampleJobProcessor : IJobProcessor<SampleJobStep>
    {
        [ComponentPlug]
        public IJobQueue<SampleJobStep> JobQueue { get; set; }


        private static int _index;
        public void Initialize(JobData jobData)
        {
        }

        public async Task<JobProcessingResult> Process(List<SampleJobStep> items)
        {
            var initialStep = new SampleJobStep
            {
                Number = _index
            };

            await JobQueue.Enqueue(initialStep, "sample-job");

            _index++;
            return null;
        }
        
        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }
}