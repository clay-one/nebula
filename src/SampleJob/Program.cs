using System;
using System.Collections.Generic;
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
        private static IJobManager _jobManager;

        private static void Main()
        {
            Nebula.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);

            Nebula.MongoConnectionString = "mongodb://localhost:27017/SampleJob";
            Nebula.RedisConnectionString = "localhost:6379";
            Nebula.KafkaConfig = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("bootstrap.servers", "172.30.3.59:9101"),
                new KeyValuePair<string, object>("group.id", "testGroup"),
                new KeyValuePair<string, object>("auto.commit.interval.ms", 5000),
                new KeyValuePair<string, object>("enable.auto.commit", true),
                new KeyValuePair<string, object>("statistics.interval.ms", 60000),
                new KeyValuePair<string, object>("auto.offset.reset", "earliest"),
                new KeyValuePair<string, object>("queue.buffering.max.ms", 1),
                new KeyValuePair<string, object>("batch.num.messages", 1),
                new KeyValuePair<string, object>("fetch.wait.max.ms", 5000)
            };

            _jobManager = Nebula.GetJobManager();

            CreateKafkaJob().Wait();
            //CreateJob().Wait();
        }

        private static async Task CreateJob()
        {
            try
            {
                var jobId = await _jobManager.CreateNewJobOrUpdateDefinition<SampleJobStep>(
                    string.Empty, "sample-job", nameof(SampleJobStep), new JobConfigurationData
                    {
                        MaxBatchSize = 1,
                        MaxConcurrentBatchesPerWorker = 5,
                        IsIndefinite = false,
                        MaxBlockedSecondsPerCycle = 300,
                        QueueTypeName = QueueType.Delayed
                    });

                var queue = Nebula.JobStepSourceBuilder.BuildDelayedJobQueue<SampleJobStep>(jobId);

                var processTime = DateTime.UtcNow;
                await queue.Enqueue(new SampleJobStep {Number = 1}, processTime.AddSeconds(-5));
                await queue.Enqueue(new SampleJobStep {Number = 2}, processTime.AddSeconds(1));
                //await queue.Enqueue(new SampleJobStep {Number = 3}, processTime);
                //await queue.Enqueue(new SampleJobStep {Number = 4}, processTime);

                await _jobManager.StartJobIfNotStarted(string.Empty, nameof(SampleJobStep));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task CreateKafkaJob()
        {
            try
            {
                Nebula.RegisterJobQueue(typeof(KafkaJobQueue<>), QueueType.Kafka);

                var jobId = await _jobManager.CreateNewJobOrUpdateDefinition<SampleJobStep>(
                    string.Empty, "sample-job", nameof(SampleJobStep), new JobConfigurationData
                    {
                        MaxBatchSize = 10,
                        MaxConcurrentBatchesPerWorker = 5,
                        IsIndefinite = true,
                        MaxBlockedSecondsPerCycle = 300,
                        QueueTypeName = QueueType.Kafka,
                        IdleSecondsToCompletion = 60
                    });

                var queue = Nebula.JobStepSourceBuilder.BuildKafkaJobQueue<SampleJobStep>(jobId);

                for (var index = 1; index <= 200; index++)
                    queue.Enqueue( new SampleJobStep {Number = index});

                await _jobManager.StartJobIfNotStarted(string.Empty, jobId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}