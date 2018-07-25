using System;
using ComposerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Storage;
using Test.Mock;

namespace Test
{
    public abstract class TestClassBase
    {
        protected  NebulaContext NebulaContext = new NebulaContext();
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            ConfigureNebula();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.ComponentContext.Unregister(new ContractIdentity(typeof(IJobStore)));
            NebulaContext.ComponentContext.Register(typeof(IJobStore), typeof(MockJobStore));

            NebulaContext.ComponentContext.Unregister(new ContractIdentity(typeof(IJobNotification)));
            NebulaContext.ComponentContext.Register(typeof(IJobNotification), typeof(MockJobNotification));
        }
    }
}