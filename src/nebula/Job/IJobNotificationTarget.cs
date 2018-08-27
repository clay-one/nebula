using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job
{
    [Contract]
    public interface IJobNotificationTarget
    {
        Task ProcessNotification(string jobId);
    }
}