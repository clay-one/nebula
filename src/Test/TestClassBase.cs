using ComposerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Connection;
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

        protected virtual void ConfigureNebula()
        {
            RegisterMockJobStore();
            RegisterMockJobNotification();
            RegisterMockBackgroundTaskScheduler();
        }

        protected void RegisterMockBackgroundTaskScheduler()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IBackgroundTaskScheduler)));
            Nebula.ComponentContext.Register(typeof(IBackgroundTaskScheduler), typeof(MockBackgroundTaskScheduler));
        }

        protected void RegisterMockJobNotification()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobNotification)));
            Nebula.ComponentContext.Register(typeof(IJobNotification), typeof(MockJobNotification));
        }

        protected void RegisterMockJobStore()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IJobStore)));
            Nebula.ComponentContext.Register(typeof(IJobStore), typeof(MockJobStore));
        }

        protected void RegisterMockRedisManager()
        {
            Nebula.ComponentContext.Unregister(new ContractIdentity(typeof(IRedisConnectionManager)));
            Nebula.ComponentContext.Register(typeof(IRedisConnectionManager), typeof(MockRedisManager));
        }
    }
}