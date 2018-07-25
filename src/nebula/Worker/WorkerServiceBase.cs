using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Attributes;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Job;

namespace Nebula.Worker
{
    public abstract class WorkerServiceBase
    {

        protected bool Stopping { get; set; }
        protected async Task StartAsync()
        {
            ConfigWorker(NebulaContext.ComponentContext);
            
            // Make sure all static jobs are defined on the database
            foreach (var component in NebulaContext.ComponentContext.GetAllComponents<IStaticJob>())
            {
                await component.EnsureJobsDefined();
            }


            await NebulaContext.ComponentContext.GetComponent<IJobManager>().CleanupOldJobs();

            // Watch on notification channel, to get notified of job changes immediately
            await NebulaContext.ComponentContext.GetComponent<IJobNotification>().StartNotificationTargetThread();

            // Start a runner for ongoing jobs on startup
            await NebulaContext.ComponentContext.GetComponent<IJobRunnerManager>().CheckStoreJobs();

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
                        NebulaContext.ComponentContext.GetComponent<IJobRunnerManager>().CheckStoreJobs().GetAwaiter().GetResult();
                        NebulaContext.ComponentContext.GetComponent<IJobRunnerManager>().CheckHealthOfAllRunners().GetAwaiter().GetResult();

                    }
                    catch (Exception e)
                    {
                        // TODO: Log exception
                    }
                }
            }).Start();
        }

        protected virtual void ConfigWorker(IComponentContext composer) { }

        protected async Task StopAsync()
        {
            await NebulaContext.ComponentContext.GetComponent<IJobNotification>().StopNotificationTargetThread();

            NebulaContext.ComponentContext.GetComponent<IJobRunnerManager>().StopAllRunners();
        }

    }
}
