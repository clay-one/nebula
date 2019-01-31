using System;
using System.Collections.Generic;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using SampleJob;

namespace SampleWorker
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Abaci.JobQueue.Worker worker service...");

            var nebulaContext = new NebulaContext();
            nebulaContext.RegisterJobQueue(typeof(DelayedJobQueue<>), QueueType.Delayed);
            nebulaContext.RegisterJobQueue(typeof(KafkaJobQueue<>), QueueType.Kafka);


            // register processor by type
            // nebulaContext.RegisterJobProcessor(typeof(SampleJobProcessor),typeof(SampleJobStep));

            //register processor object
            nebulaContext.RegisterJobProcessor(new SampleJobProcessor(), typeof(SampleJobStep));

            nebulaContext.MongoConnectionString = "mongodb://localhost:27017/SampleJob";
            nebulaContext.RedisConnectionString = "localhost:6379";
            nebulaContext.KafkaConfig = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("bootstrap.servers", "172.30.3.59:9101"),
                new KeyValuePair<string, object>("group.id", "testGroup"),
                new KeyValuePair<string, object>("auto.commit.interval.ms", 5000),
                new KeyValuePair<string, object>("enable.auto.commit", true),
                new KeyValuePair<string, object>("statistics.interval.ms", 60000),
                new KeyValuePair<string, object>("auto.offset.reset", "earliest"),
                new KeyValuePair<string, object>("queue.buffering.max.ms", 1),
                new KeyValuePair<string, object>("batch.num.messages", 1),
                new KeyValuePair<string, object>("fetch.wait.max.ms", 5000),
                new KeyValuePair<string, object>("fetch.min.bytes", 1),
            };


            nebulaContext.StartWorkerService();
            
            Console.WriteLine("Service started. Press ENTER to stop.");
            Console.ReadLine();

            Console.WriteLine("Stopping the serivce...");
            nebulaContext.StopWorkerService();
            Console.WriteLine("Service stopped, everything looks clean.");
        }
    }
}