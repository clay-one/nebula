using System;
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


            // register processor by type
            // nebulaContext.RegisterJobProcessor(typeof(SampleJobProcessor),typeof(SampleJobStep));

            //register processor object
            nebulaContext.RegisterJobProcessor(new SampleJobProcessor(), typeof(SampleJobStep));

            nebulaContext.MongoConnectionString = "mongodb://localhost:27017/SampleJob";
            nebulaContext.RedisConnectionString = "localhost:6379";

            nebulaContext.StartWorkerService();

            var queue = nebulaContext.GetDelayedJobQueue<SampleJobStep>(QueueType.Delayed);

            Console.WriteLine("Service started. Press ENTER to stop.");
            Console.ReadLine();

            Console.WriteLine("Stopping the serivce...");
            nebulaContext.StopWorkerService();
            Console.WriteLine("Service stopped, everything looks clean.");
        }
    }
}