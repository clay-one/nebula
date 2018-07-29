using ComposerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Storage;
using Test.Mock;

namespace Test
{
    public abstract class TestClassBase
    {
        protected static NebulaContext Nebula;

        [TestInitialize]
        public void ClassInit()
        {
            Nebula = new NebulaContext();
            ConfigureNebula();
        }

        protected static void ConfigureNebula()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobStore)));
            Nebula.ComponentContext.Register(typeof(IJobStore), typeof(MockJobStore));

            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobNotification)));
            Nebula.ComponentContext.Register(typeof(IJobNotification), typeof(MockJobNotification));
        }
    }
}