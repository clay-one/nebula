using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Worker;

[assembly: InternalsVisibleTo("Test")]

namespace Nebula
{
    public class NebulaContext
    {
        public NebulaContext()
        {
            if (ComponentContext == null)
                ConfigureComposer();
        }

        internal IComponentContext ComponentContext { get; set; }

        public void RegisterJobProcessor(Type processor)
        {
            ComponentContext.Register(typeof(IJobProcessor<>), processor);
        }

        public void RegisterJobQueue(Type jobQueue, string queueName)
        {
            ComponentContext.Register(typeof(IJobQueue<>), queueName, jobQueue);
        }

        public TJobQueue GetJobQueue<TJobQueue>(Type stepType, string queueName) where TJobQueue : class
        {
            var contract = typeof(IJobQueue<>).MakeGenericType(stepType);
            return ComponentContext.GetComponent(contract, queueName) as TJobQueue;
        }

        public IJobManager GetJobManager()
        {
            return ComponentContext.GetComponent<IJobManager>();
        }

        public void StartWorkerService()
        {
            var workerService = ComponentContext.GetComponent<WorkerService>();
            workerService.StartAsync().GetAwaiter().GetResult();
        }

        public void StopWorkerService()
        {
            var workerService = ComponentContext.GetComponent<WorkerService>();

            workerService.Stopping = true;
            workerService.StopAsync().GetAwaiter().GetResult();
        }

        public void ConnectionConfig(string path)
        {
            if (!string.IsNullOrEmpty(path))
                ComponentContext.ProcessCompositionXml(path);
        }

        private void ConfigureComposer()
        {
            var context = new ComponentContext();

            var assembly = Assembly.Load("Nebula");
            context.RegisterAssembly(assembly);

            context.Configuration.DisableAttributeChecking = true;
            ComponentContext = context;
        }

    }
}