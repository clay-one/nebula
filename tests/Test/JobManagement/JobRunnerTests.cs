using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class JobRunnerTests : TestClassBase
    {
        private IServiceProvider _sp;
        [TestInitialize]
        public void TestInit()
        {
            _sp = new ServiceCollection()
                .AddScoped<ScopedService1>()
                .AddTransient<TransientService1>()
                .AddTransient<IJobProcessor<FirstJobStep>, TransientProcessor1>()
                .AddTransient<TransientProcessor1>()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false });
        }

        [TestMethod]
        public void JobProcessor_InstanceProvider_Registration_ShouldReturnDifferentInstances()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            Nebula.RegisterJobProcessor(() => _sp.GetService<TransientProcessor1>(),
                typeof(FirstJobStep));

            var jobProcessor1 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();
            var jobProcessor2 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();

            Assert.AreNotEqual(jobProcessor2, jobProcessor1);
        }

        [TestMethod]
        public void JobProcessor_InstanceProvider_ReturnsInterface_ShouldReturnDifferentInstances()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            Nebula.RegisterJobProcessor(() => _sp.GetService<IJobProcessor<FirstJobStep>>(),
                typeof(FirstJobStep));

            var jobProcessor1 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();
            var jobProcessor2 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();

            Assert.AreNotEqual(jobProcessor2, jobProcessor1);
        }

        [TestMethod]
        public void JobProcessor_Instance_Registration_ShouldReturnSameInstances()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            Nebula.RegisterJobProcessor(typeof(TransientProcessor1), typeof(FirstJobStep));

            var jobProcessor1 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();
            var jobProcessor2 = Nebula.ComponentContext.GetComponent<IJobProcessor<FirstJobStep>>();

            Assert.AreEqual(jobProcessor2, jobProcessor1);
        }
    }

    public class TransientProcessor1 : IJobProcessor<FirstJobStep>
    {
        private readonly TransientService1 _transientService1;

        public TransientProcessor1()
        {
            _transientService1 = new TransientService1();
        }
        public TransientProcessor1(TransientService1 transientService1)
        {
            _transientService1 = transientService1;
        }

        public JobData JobData { get; set; }
        public Guid Guid = Guid.NewGuid();

        public void Initialize(JobData jobData, NebulaContext nebulaContext)
        {
            JobData = jobData;
            Trace.WriteLine($@"
                            Initialization:
                                guid: {Guid}
                                jobId: {JobData.JobId}
                                _transientService1: {_transientService1.Guid}");

        }

        public Task<JobProcessingResult> Process(List<FirstJobStep> items)
        {
            Trace.WriteLine($@"
                            Process:
                                guid: {Guid}
                                jobId: {JobData.JobId}
                                _transientService1: {_transientService1.Guid}");
            return Task.FromResult(new JobProcessingResult());
        }

        public Task<long> GetTargetQueueLength()
        {
            return Task.FromResult(0L);
        }
    }

    public class TransientService1
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
    }

    public class ScopedService1
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
    }
}