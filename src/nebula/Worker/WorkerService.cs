using System;
using System.Threading;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Attributes;
using Nebula.Job;

namespace Nebula.Worker
{
    [Contract]
    [Component]
    public class WorkerService
    {
        [ComponentPlug]
        public IComposer Composer { get; set; }

        public bool Stopping { get; set; }

        public async Task StartAsync()
        {
            // Make sure all static jobs are defined on the database
            foreach (var component in Composer.GetAllComponents<IStaticJob>())
                await component.EnsureJobsDefined();

            await Composer.GetComponent<IJobManager>().CleanupOldJobs();

            // Watch on notification channel, to get notified of job changes immediately
            await Composer.GetComponent<IJobNotification>().StartNotificationTargetThread();

            // Start a runner for ongoing jobs on startup
            await Composer.GetComponent<IJobRunnerManager>().CheckStoreJobs();

            // Start a monitor thread
            new Thread(() =>
            {
                while (true)
                {
                    for (var i = 0; i < 60; i++)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        if (Stopping)
                            break;
                    }

                    if (Stopping)
                        break;

                    try
                    {
                        Composer.GetComponent<IJobRunnerManager>().CheckStoreJobs().GetAwaiter().GetResult();
                        Composer.GetComponent<IJobRunnerManager>().CheckHealthOfAllRunners().GetAwaiter()
                            .GetResult();
                    }
                    catch
                    {
                        // TODO: Log exception
                    }
                }
            }).Start();
        }
        
        public async Task StopAsync()
        {
            await Composer.GetComponent<IJobNotification>().StopNotificationTargetThread();

            Composer.GetComponent<IJobRunnerManager>().StopAllRunners();
        }
    }
}