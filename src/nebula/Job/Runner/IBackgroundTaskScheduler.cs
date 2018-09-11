using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job.Runner
{
    [Contract]
    public interface IBackgroundTaskScheduler
    {
        Task Run(Func<Task> function);
    }
}