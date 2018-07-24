using System.Reflection;
using ComposerCore;
using ComposerCore.Utility;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Worker;

namespace SampleWorker
{
    public class JobQueueWorkerService : WorkerServiceBase
    {
        protected override void ConfigWorker(IComponentContext composer)
        {
            RunCompositionXml(composer, string.Empty, string.Empty, "Connections.config");
            RunCompositionXml(composer, "SampleWorker", "SampleWorker.Composition.xml", string.Empty);

            composer.Configuration.DisableAttributeChecking = true;
            composer.Register(typeof(IJobQueue<>), typeof(RedisJobQueue<>));

        }

        private static void RunCompositionXml(IComponentContext composer, string assemblyName,
            string manifestResourceName, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                composer.ProcessCompositionXml(path);
                return;
            }

            var assembly = Assembly.Load(assemblyName);
            composer.ProcessCompositionXmlFromResource(assembly, manifestResourceName);
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