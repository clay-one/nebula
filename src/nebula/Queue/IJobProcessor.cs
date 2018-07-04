using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Storage.Model;

namespace Nebula.Queue
{
    [Contract]
    public interface IJobProcessor<TItem> where TItem : IJobStep
    {
        void Initialize(JobData jobData);
        Task<JobProcessingResult> Process(List<TItem> items);
        Task<long> GetTargetQueueLength();
    }
}