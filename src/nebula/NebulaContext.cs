using System;
using System.Reflection;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Nebula.Queue;

namespace Nebula
{
    public static class NebulaContext
    {
        static NebulaContext()
        {
            if (ComponentContext == null)
                ConfigureComposer();
        }

        public static IComponentContext ComponentContext { get; set; }

        public static void Register(Type contractType, Type componentType)
        {
            ComponentContext.Register(contractType, componentType);
        }

        public static void Register(Type contractType, Type componentType, string name)
        {
            ComponentContext.Register(contractType, name, componentType);
        }

        public static TContract GetComponent<TContract>() where TContract : class
        {
            return GetComponent<TContract>(null);
        }

        public static TContract GetComponent<TContract>(string name) where TContract : class
        {
            return ComponentContext.GetComponent(typeof(TContract), name) as TContract;
        }

        public static IJobQueue GetJobQueue(Type stepType, string queueName)
        {
            var contract = typeof(IJobQueue<>).MakeGenericType(stepType);
            return NebulaContext.ComponentContext.GetComponent(contract, queueName) as IJobQueue;
        }

        public static void ConnectionConfig(string path)
        {
            if (!string.IsNullOrEmpty(path))
                ComponentContext.ProcessCompositionXml(path);
        }

        private static void ConfigureComposer()
        {
            var context = new ComponentContext();

            var assembly = Assembly.Load("Nebula");
            context.RegisterAssembly(assembly);

            context.ProcessCompositionXml("Connections.config");

            context.Configuration.DisableAttributeChecking = true;
            ComponentContext = context;
        }
    }
}