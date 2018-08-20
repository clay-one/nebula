using System;
using System.Threading.Tasks;
using hydrogen.General.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Multitenancy;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class CreateNewJobOrUpdateDefinitionTests : TestClassBase
    {
        [TestMethod]
        public async Task CreateJob_NewJob_ShouldExistInStore()
        {
            var tenant = new NullTenant();
            tenant.GetCurrentTenant();

            var jobId = "testJobId";
            var jobName = "testJob";

            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(tenant.Id, jobName, jobId,
                new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(tenant.Id, "secondJob", "secondJob",
                new JobConfigurationData { QueueTypeName = QueueType.InMemory });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(tenant.Id, jobId);

            Assert.IsNotNull(job);
            Assert.AreEqual(jobId, job.JobId);
        }

        [TestMethod]
        public async Task CreateJob_jobWithoutJobId_ShouldHaveValue()
        {
            var tenant = new NullTenant();
            tenant.GetCurrentTenant();

            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            Assert.IsTrue(!jobId.IsNullOrWhitespace());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_NullConfiguration_ExceptionThrown()
        {
            var tenant = new NullTenant();
            tenant.GetCurrentTenant();


            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(tenant.Id);
        }


    }
}