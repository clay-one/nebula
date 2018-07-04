using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Job;

namespace Nebula.Worker
{
    public abstract class WorkerServiceBase
    {
        private IComponentContext _composer;
        protected bool Stopping { get; set; }
        protected async Task StartAsync()
        {
            _composer = new ComponentContext();
            ConfigWorker(_composer);

            ConfigComposer(_composer);
            
            // Make sure all static jobs are defined on the database
            foreach (var component in _composer.GetAllComponents<IStaticJob>())
            {
                await component.EnsureJobsDefined();
            }


            await _composer.GetComponent<IJobManager>().CleanupOldJobs();

            // Watch on notification channel, to get notified of job changes immediately
            await _composer.GetComponent<IJobNotification>().StartNotificationTargetThread();

            // Start a runner for ongoing jobs on startup
            await _composer.GetComponent<IJobRunnerManager>().CheckStoreJobs();

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
                        _composer.GetComponent<IJobRunnerManager>().CheckStoreJobs().GetAwaiter().GetResult();
                        _composer.GetComponent<IJobRunnerManager>().CheckHealthOfAllRunners().GetAwaiter().GetResult();

                    }
                    catch (Exception e)
                    {
                        // TODO: Log exception
                    }
                }
            }).Start();
        }

        protected virtual void ConfigWorker(IComponentContext composer) { }

        private static void ConfigComposer(IComponentContext composer)
        {
            RunCompositionXml(composer, string.Empty, string.Empty, "Connections.config");
            RunCompositionXml(composer, "SampleWorker", "SampleWorker.Composition.xml", string.Empty);
        }

        private static void RunCompositionXml(IComponentContext composer, string assemblyName,
            string manifestResourceName, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                composer.ProcessCompositionXml(path);
                return;
            }

            var assembly = Assembly.Load(assemblyName);
            composer.ProcessCompositionXmlFromResource(assembly, manifestResourceName);
        }

        protected async Task StopAsync()
        {
            await _composer.GetComponent<IJobNotification>().StopNotificationTargetThread();

            _composer.GetComponent<IJobRunnerManager>().StopAllRunners();
        }

    }
}
