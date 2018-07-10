using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Queue;
using Nebula.Storage.Model;

namespace Nebula.Job
{
    public interface IJobRunner
    {
        string JobId { get; }
        bool IsProcessRunning { get; }
        bool IsProcessTerminated { get; }
        void Initialize(JobData jobData);
        Task<bool> CheckHealth();
        void StopRunner();
    }

    [Contract]
    public interface IJobRunner<TJobStep> : IJobRunner where TJobStep : IJobStep
    {
    }
}