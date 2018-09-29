using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ComposerCore;
using ComposerCore.Attributes;
using ComposerCore.Factories;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Worker;

[assembly: InternalsVisibleTo("Test")]

namespace Nebula
{
    [Contract]
    public class NebulaContext
    {
        public NebulaContext()
        {
            if (ComponentContext == null)
                ConfigureComposer();
        }

        internal IComponentContext ComponentContext { get; set; }

        public string MongoConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
        
        public void RegisterJobProcessor(Type processor, Type stepType)
        {
            var contract = typeof(IJobProcessor<>).MakeGenericType(stepType);
            ComponentContext.Register(contract, processor);
        }

        public void RegisterJobProcessor(object processor, Type stepType)
        {
            var contract = typeof(IJobProcessor<>).MakeGenericType(stepType);

            var isTypeCorrect = processor.GetType().GetInterfaces().Any(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IJobProcessor<>));

            if (!isTypeCorrect)
                throw new ArgumentException("Processor should implement IJobProcessor<>");

            ComponentContext.Register(contract, new PreInitializedComponentFactory(processor));
        }

        public void RegisterJobQueue(Type jobQueue, string queueTypeName)
        {
            ComponentContext.Register(typeof(IJobStepSource<>), queueTypeName, jobQueue);
        }

        public IJobQueue<TJobStep> GetJobQueue<TJobStep>(string queueTypeName) where TJobStep : IJobStep
        {
            return ComponentContext.GetComponent(typeof(IJobStepSource<TJobStep>), queueTypeName) as IJobQueue<TJobStep>;
        }

        public IDelayedJobQueue<TJobStep> GetDelayedJobQueue<TJobStep>(string queueTypeName) where TJobStep : IJobStep
        {
            return ComponentContext.GetComponent(typeof(IJobStepSource<TJobStep>), queueTypeName) as IDelayedJobQueue<TJobStep>;
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
            context.Register(typeof(NebulaContext), new PreInitializedComponentFactory(this));

            context.Configuration.DisableAttributeChecking = true;
            ComponentContext = context;
        }
    }
}