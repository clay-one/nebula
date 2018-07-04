using ComposerCore;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Worker;

namespace SampleWorker
{
    public class JobQueueWorkerService : WorkerServiceBase
    {
        protected override void ConfigWorker(IComponentContext composer)
        {
            composer.Configuration.DisableAttributeChecking = true;
            composer.Register(typeof(IJobQueue<>), typeof(RedisJobQueue<>));
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