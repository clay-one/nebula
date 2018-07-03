using System;
using System.Reflection;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Job;
using Nebula.Job.Runner;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace SampleJob
{
    internal class Program
    {
        public static IComposer Composer { get; set; }

        private static void Main()
        {
            Composer = ConfigureComposer();

            var jobManager = Composer.GetComponent<IJobManager>();

            CreateJob(jobManager).Wait();
        }

        private static async Task CreateJob(IJobManager jobManager)
        {
            try
            {
                var jobId = await jobManager.CreateNewJobOrUpdateDefinition<SampleJobStep>(
                    string.Empty, "sample-job", nameof(SampleJobStep), new JobConfigurationData
                    {
                        MaxBatchSize = 100,
                        MaxConcurrentBatchesPerWorker = 5,
                        IsIndefinite = true,
                        MaxBlockedSecondsPerCycle = 300
                    });

                var initialStep = new SampleJobStep
                {
                    Number = 1
                };

                await Composer.GetComponent<IJobQueue<SampleJobStep>>().Enqueue(initialStep, jobId);

                await jobManager.StartJobIfNotStarted(string.Empty, nameof(SampleJobStep));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static IComposer ConfigureComposer()
        {
            var composer = new ComponentContext();
            var assembly = Assembly.Load("SampleJob");

            composer.RegisterAssembly(assembly);
            composer.RegisterAssembly("Nebula");
            composer.ProcessCompositionXml("Connections.config");

            composer.Configuration.DisableAttributeChecking = true;
            composer.Register(typeof(IJobQueue<>), typeof(RedisJobQueue<>));

            // ReSharper disable UnusedVariable
            var jobProcessor = composer.GetComponent<IJobProcessor<SampleJobStep>>();
            var jobStore = composer.GetComponent<IJobStore>();
            var jobRunnerManager = composer.GetComponent<IJobRunnerManager>();
            var jobStatisticsCalculator = composer.GetComponent<JobStatisticsCalculator>();
            var jobNotification = composer.GetComponent<IJobNotification>();

            return composer;
        }
    }
}