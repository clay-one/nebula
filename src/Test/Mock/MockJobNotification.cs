using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Job;

namespace Test.Mock
{
    [Component]
   public class MockJobNotification: IJobNotification
    {
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
            throw new NotImplementedException();
        }
    }
}
