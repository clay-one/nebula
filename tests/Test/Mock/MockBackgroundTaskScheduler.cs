using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Job.Runner;

namespace Test.Mock
{
    [Component]
    public class MockBackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        public Task Run(Func<Task> function)
        {
            return function();
        }
    }
}