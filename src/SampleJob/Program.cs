using System;
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
            Nebula.RegisterJobQueue(typeof(RedisJobQueue<>), QueueType.Redis);

            // register processor by type
            // Nebula.RegisterJobProcessor(typeof(SampleJobProcessor), typeof(SampleJobStep));

            // register processor object
            Nebula.RegisterJobProcessor(new SampleJobProcessor(), typeof(SampleJobStep));

            Nebula.MongoConnectionString = "mongodb://localhost:27017/SampleJob";
            Nebula.RedisConnectionString = "localhost:6379";

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
                        QueueTypeName = QueueType.Redis
                    });

                var initialStep = new SampleJobStep
                {
                    Number = 1
                };

                await Nebula.GetJobQueue<SampleJobStep>(QueueType.Redis).Enqueue(initialStep, jobId);

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