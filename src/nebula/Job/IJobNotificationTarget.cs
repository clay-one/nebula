using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job
{
    [Contract]
    internal interface IJobNotificationTarget
    {
        Task ProcessNotification(string jobId);
    }
}