using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Connection;
using Nebula.Job;
using ServiceStack.Text;

namespace Nebula.Queue.Implementation
{
    [Component]
    internal class RedisJobNotification : IJobNotification
    {
        private const string JobUpdatedChannelName = "job-updated";
//        private Thread _targetThread;

        [ComponentPlug]
        public IRedisConnectionManager RedisManager { get; set; }

        [ComponentPlug]
        public IJobNotificationTarget NotificationTarget { get; set; }

        public Task StartNotificationTargetThread()
        {
            return RedisManager.GetSubscriber().SubscribeAsync(JobUpdatedChannelName, (channel, value) =>
            {
                $"Got notification on JobId '{value}' from channel '{channel}'".Print();
                NotificationTarget.ProcessNotification(value).GetAwaiter().GetResult();
            });
        }

        public Task StopNotificationTargetThread()
        {
            return RedisManager.GetSubscriber().UnsubscribeAsync(JobUpdatedChannelName);
        }

        public async Task NotifyJobUpdated(string jobId)
        {
            await RedisManager.GetSubscriber().PublishAsync(JobUpdatedChannelName, jobId);
        }
    }
}