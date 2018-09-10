using ComposerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Job.Runner;
using Nebula.Multitenancy;
using Nebula.Storage;
using Test.Mock;

namespace Test
{
    public abstract class TestClassBase
    {
        protected static NebulaContext Nebula;
        protected readonly NullTenant Tenant = new NullTenant();


        [TestInitialize]
        public void ClassInit()
        {
            Tenant.GetCurrentTenant();
            Nebula = new NebulaContext();
            ConfigureNebula();
        }

        protected static void ConfigureNebula()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobStore)));
            Nebula.ComponentContext.Register(typeof(IJobStore), typeof(MockJobStore));

            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobNotification)));
            Nebula.ComponentContext.Register(typeof(IJobNotification), typeof(MockJobNotification));

            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IBackgroundTaskScheduler)));
            Nebula.ComponentContext.Register(typeof(IBackgroundTaskScheduler), typeof(MockBackgroundTaskScheduler));
        }
    }
}