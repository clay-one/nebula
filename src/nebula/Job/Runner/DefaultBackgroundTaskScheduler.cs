using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job.Runner
{
    [Component]
    public class DefaultBackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        public Task Run(Func<Task> function)
        {
            return Task.Run(function);
        }
    }
}
