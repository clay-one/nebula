using System.Reflection;
using ComposerCore;
using ComposerCore.Utility;
using Nebula;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Worker;

namespace SampleWorker
{
    public class JobQueueWorkerService : WorkerServiceBase
    {
        protected override void ConfigWorker(IComponentContext composer)
        {
            var nebulaContext = new NebulaContext();

            nebulaContext.ConnectionConfig("Connections.config");
        }
        
        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }

        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }
}