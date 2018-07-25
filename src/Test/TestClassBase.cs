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
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            ConfigureNebula();
        }

        private static void ConfigureNebula()
        {
            NebulaContext.ComponentContext.Unregister(new ContractIdentity(typeof(IJobStore)));
            NebulaContext.Register(typeof(IJobStore), typeof(MockJobStore));

            NebulaContext.ComponentContext.Unregister(new ContractIdentity(typeof(IJobNotification)));
            NebulaContext.Register(typeof(IJobNotification), typeof(MockJobNotification));
        }
    }
}