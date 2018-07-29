using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nebula;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;

namespace SampleJob
{
    internal class Program
    {
        public static NebulaContext  Nebula = new NebulaContext();
        private static void Main()
        {
           // Nebula.RegisterJobQueue(typeof(SampleJobQueue), nameof(SampleJobQueue));
            Nebula.RegisterJobQueue(typeof(RedisJobQueue<>), "RedisJobQueue");
            Nebula.ConnectionConfig("Connections.config");

            var jobManager = Nebula.GetJobManager();

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
                        MaxBlockedSecondsPerCycle = 300,
                        QueueName = "RedisJobQueue"
                    });

                var initialStep = new SampleJobStep
                {
                    Number = 1
                };

                await Nebula.GetJobQueue<IJobQueue<SampleJobStep>>(typeof(SampleJobStep), "RedisJobQueue")
                    .Enqueue(initialStep, jobId);

                await jobManager.StartJobIfNotStarted(string.Empty, nameof(SampleJobStep));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}