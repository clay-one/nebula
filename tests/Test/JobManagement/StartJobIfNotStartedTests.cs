using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class StartJobIfNotStartedTests : TestClassBase
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StartJobIfNotStarted_NotDefinedJob_ExceptionThrown()
        {
            await Nebula.GetJobManager().StartJobIfNotStarted(Tenant.Id, "jobId");
        }

        [TestMethod]
        public async Task StartJobIfNotStarted_StartJob_StateShouldBeInProgress()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            await jobManager.StartJobIfNotStarted(Tenant.Id, jobId);

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var JobData = await jobStore.Load(Tenant.Id, jobId);

            Assert.IsNotNull(JobData);
            Assert.AreEqual(JobState.InProgress, JobData.Status.State);
        }

        [TestMethod]
        public async Task StartJobIfNotStarted_StartNewJob_ShouldCreateRunner()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

             jobManager.StartJobIfNotStarted(Tenant.Id, jobId).Wait();

            Task.Delay(1000).Wait();
            var jobRunnerManager = Nebula.ComponentContext.GetComponent<IJobRunnerManager>();
            
            Assert.IsTrue(jobRunnerManager.IsJobRunnerStarted(jobId));
        }
    }
}