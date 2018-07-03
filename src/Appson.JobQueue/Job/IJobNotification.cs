using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job
{
    [Contract]
    public interface IJobNotification
    {
        Task StartNotificationTargetThread();
        Task StopNotificationTargetThread();

        Task NotifyJobUpdated(string jobId);
    }
}