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
        public static NebulaContext Nebula = new NebulaContext();

        private static void Main()
        {
            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);
            
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
                        MaxBatchSize = 1,
                        MaxConcurrentBatchesPerWorker = 5,
                        IsIndefinite = false,
                        MaxBlockedSecondsPerCycle = 300,
                        QueueTypeName = QueueType.Delayed
                    });

                var initialStep = new SampleJobStep
                {
                    Number = 1
                };

                var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<SampleJobStep>(jobId);

                var processTime = DateTime.UtcNow;
                await queue.Enqueue(initialStep, processTime);
                await queue.Enqueue(new SampleJobStep {Number = 2}, processTime);
                await queue.Enqueue(new SampleJobStep {Number = 3}, processTime);
                await queue.Enqueue(new SampleJobStep {Number = 4}, processTime);

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