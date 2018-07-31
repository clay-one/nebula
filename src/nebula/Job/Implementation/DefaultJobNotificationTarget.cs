using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job.Implementation
{
    [Component]
    internal class DefaultJobNotificationTarget : IJobNotificationTarget
    {
        [ComponentPlug]
        public IJobRunnerManager RunnerManager { get; set; }

        public async Task ProcessNotification(string jobId)
        {
            await RunnerManager.CheckHealthOrCreateRunner(jobId);
        }
    }
}