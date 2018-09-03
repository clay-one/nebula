using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Job;

namespace Test.Mock
{
    [Component]
   public class MockJobNotification: IJobNotification
    {
        [ComponentPlug]
        public IJobNotificationTarget NotificationTarget { get; set; }

        public Task StartNotificationTargetThread()
        {
            throw new NotImplementedException();
        }

        public Task StopNotificationTargetThread()
        {
            throw new NotImplementedException();
        }

        public Task NotifyJobUpdated(string jobId)
        {
            NotificationTarget.ProcessNotification(jobId).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }
    }
}
