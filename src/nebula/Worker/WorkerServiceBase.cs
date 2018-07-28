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
    public class WorkerServiceBase
    {
        [ComponentPlug]
        public IComponentContext ComponentContext { get; set; }

        protected bool Stopping { get; set; }

        protected async Task StartAsync()
        {
            ConfigWorker(ComponentContext);

            // Make sure all static jobs are defined on the database
            foreach (var component in ComponentContext.GetAllComponents<IStaticJob>())
                await component.EnsureJobsDefined();

            await ComponentContext.GetComponent<IJobManager>().CleanupOldJobs();

            // Watch on notification channel, to get notified of job changes immediately
            await ComponentContext.GetComponent<IJobNotification>().StartNotificationTargetThread();

            // Start a runner for ongoing jobs on startup
            await ComponentContext.GetComponent<IJobRunnerManager>().CheckStoreJobs();

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
                        ComponentContext.GetComponent<IJobRunnerManager>().CheckStoreJobs().GetAwaiter().GetResult();
                        ComponentContext.GetComponent<IJobRunnerManager>().CheckHealthOfAllRunners().GetAwaiter()
                            .GetResult();
                    }
                    catch (Exception e)
                    {
                        // TODO: Log exception
                    }
                }
            }).Start();
        }

        protected virtual void ConfigWorker(IComponentContext composer)
        {
        }

        protected async Task StopAsync()
        {
            await ComponentContext.GetComponent<IJobNotification>().StopNotificationTargetThread();

            ComponentContext.GetComponent<IJobRunnerManager>().StopAllRunners();
        }
    }
}