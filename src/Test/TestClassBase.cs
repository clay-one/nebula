using System;
using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nebula.Connection;
using Nebula.Connection.Implementation;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Implementation;
using Test.Mock;

namespace Test
{
    public class TestClassBase
    {
        protected IComponentContext Composer { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            ConfigureComposer();
        }

        private void ConfigureComposer()
        {
            if (Composer != null)
                return;

            var composer = new ComponentContext();

            composer.RegisterAssembly("Nebula");
            composer.ProcessCompositionXml("Connections.config");

            composer.Unregister(new ContractIdentity(typeof(IJobStore)));
            composer.Register(typeof(IJobStore), typeof(MockJobStore));

            composer.Unregister(new ContractIdentity(typeof(IJobNotification)));
            composer.Register(typeof(IJobNotification), typeof(MockJobNotification));

            Composer = composer;
        }

        protected IJobQueue GetJobQueue(Type stepType, string queueName)
        {
            var contract = typeof(IJobQueue<>).MakeGenericType(stepType);
            return Composer.GetComponent(contract, queueName) as IJobQueue;
        }
    }
}