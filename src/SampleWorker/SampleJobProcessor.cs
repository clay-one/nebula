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
        private static int _index;
        private NebulaContext _nebulaContext;

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            _nebulaContext = nebulaContext;
        }

        public async Task<JobProcessingResult> Process(List<SampleJobStep> items)
        {
            return new JobProcessingResult();
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }

        public async Task JobCompleted()
        {
            
        }
    }
}