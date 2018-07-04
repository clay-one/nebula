using System;
using log4net;
using log4net.Config;

namespace SampleWorker
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Abaci.JobQueue.Worker worker service...");

            var service = new JobQueueWorkerService();
            service.Start();
            Console.WriteLine("Service started. Press ENTER to stop.");
            Console.ReadLine();

            Console.WriteLine("Stopping the serivce...");
            service.Stop();
            Console.WriteLine("Service stopped, everything looks clean.");
        }
    }
}
