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
    public class StartJobTests : TestClassBase
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StartJob_NotDefinedJob_ExceptionThrown()
        {
            await Nebula.GetJobManager().StartJob(Tenant.Id, "jobId");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task StartJob_InvalidJobState_ExceptionThrown()
        {
            var jobId = "jobId";
            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            await jobStore.AddOrUpdateDefinition(new JobData
            {
                TenantId = Tenant.Id,
                JobId = jobId,
                Status = new JobStatusData {State = JobState.Failed}
            });

            await Nebula.GetJobManager().StartJob(Tenant.Id, jobId);
        }

        [TestMethod]
        public async Task StartJob_StartNewJob_ShouldCreateRunner()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            await jobManager.StartJob(Tenant.Id, jobId);

            var jobRunnerManager = Nebula.ComponentContext.GetComponent<IJobRunnerManager>();

            Assert.IsTrue(jobRunnerManager.IsJobRunnerStarted(jobId));
        }

        [TestMethod]
        public async Task StartJob_EnqueueOneItem_QueueShouldBeEmpty()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            var queue = Nebula.JobStepSourceBuilder.BuildInMemoryJobQueue<FirstJobStep>(jobId);
            await queue.Enqueue(new FirstJobStep {Number = 1});

            var initialLength = await queue.GetQueueLength();

            await jobManager.StartJob(Tenant.Id, jobId);


            var processedLength = await queue.GetQueueLength();

            var jobRunnerManager = Nebula.ComponentContext.GetComponent<IJobRunnerManager>();

            Assert.IsTrue(jobRunnerManager.IsJobRunnerStarted(jobId));
            Assert.AreEqual(1, initialLength);
            Assert.AreEqual(0, processedLength);
        }
    }
}