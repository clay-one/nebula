﻿using System;
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
            nebulaContext.RegisterJobQueue(typeof(RedisJobQueue<>), QueueType.Redis);
            nebulaContext.RegisterJobProcessor(typeof(SampleJobProcessor),typeof(SampleJobStep));
            nebulaContext.ConnectionConfig("Connections.config");

            nebulaContext.StartWorkerService();

            Console.WriteLine("Service started. Press ENTER to stop.");
            Console.ReadLine();

            Console.WriteLine("Stopping the serivce...");
            nebulaContext.StopWorkerService();
            Console.WriteLine("Service stopped, everything looks clean.");
        }
    }
}