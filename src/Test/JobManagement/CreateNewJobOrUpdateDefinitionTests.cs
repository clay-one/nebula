using System.Threading.Tasks;
using hydrogen.General.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class CreateNewJobOrUpdateDefinitionTests : TestClassBase
    {
        [TestMethod]
        public async Task CreateJob_NewJob_ShouldExistInStore()
        {
            var jobId = "testJobId";
            var jobName = "testJob";

            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id, jobName, jobId,
                new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id, "secondJob", "secondJob",
                new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsNotNull(job);
            Assert.AreEqual(jobId, job.JobId);
        }

        [TestMethod]
        public async Task CreateJob_jobWithoutJobId_ShouldHaveValue()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueType.InMemory});

            Assert.IsTrue(!jobId.IsNullOrWhitespace());
        }

        [TestMethod]
        public async Task CreateJob_NewJob_QueueExistenceCheck()
        {
            Nebula.RegisterJobQueue(typeof(FirstJobQueue<FirstJobStep>), QueueTypes.FirstJobQueue);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData {QueueTypeName = QueueTypes.FirstJobQueue });

            var queue = Nebula.GetJobQueue<FirstJobStep>(QueueTypes.FirstJobQueue) as FirstJobQueue<FirstJobStep>;

            Assert.IsNotNull(queue);
            Assert.IsTrue(queue.QueueExistenceChecked);
        }
    }
}