using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class StopJobTests : TestClassBase
    {
        private readonly string _jobId = "jobId";

        [TestMethod]
        public async Task StopJob_NotDefinedJob_InvalidJobIdErrorKey()
        {
            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, "jobId");

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobId, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_IndefiniteJob_InvalidJobActionErrorKey()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    IsIndefinite = true,
                    QueueTypeName = QueueType.InMemory
                });

            var result = await jobManager.StopJob(Tenant.Id, jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobAction, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_InCompletePreprocessor_JobActionHasPreprocessorDependencyErrorKey()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var preprocessorJobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    QueueTypeName = QueueType.InMemory
                });

            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    QueueTypeName = QueueType.InMemory
                });

            await jobManager.AddPredecessor(Tenant.Id, jobId, preprocessorJobId);

            var result = await jobManager.StopJob(Tenant.Id, jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.JobActionHasPreprocessorDependency, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_CompletedJob_InvalidJobStateErrorKey()
        {
            await AddJobToDb(JobState.Completed);

            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, _jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobState, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_FailedJob_InvalidJobStateErrorKey()
        {
            await AddJobToDb(JobState.Failed);

            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, _jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobState, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_ExpiredJob_InvalidJobStateErrorKey()
        {
            await AddJobToDb(JobState.Expired);

            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, _jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobState, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_StoppedJob_Success()
        {
            await AddJobToDb(JobState.Stopped);

            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, _jobId);

            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public async Task StopJob_InitializingJob_InvalidJobStateErrorKey()
        {
            await AddJobToDb(JobState.Initializing);

            var result = await Nebula.GetJobManager().StopJob(Tenant.Id, _jobId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(ErrorKeys.InvalidJobState, result.Errors.FirstOrDefault()?.ErrorKey);
        }

        [TestMethod]
        public async Task StopJob_StopStartedJob_RunnersShouldStop()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    QueueTypeName = QueueType.InMemory
                });

            await jobManager.StartJob(Tenant.Id, jobId);

            await jobManager.StopJob(Tenant.Id, jobId);

            var jobRunnerManager = Nebula.ComponentContext.GetComponent<IJobRunnerManager>();

            Assert.IsTrue(jobRunnerManager.IsJobRunnerStoped(jobId));
        }

        [TestMethod]
        public async Task StopJob_StopStartedJob_ShouldPurgeQueue()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    QueueTypeName = QueueType.InMemory
                });

            var queue = Nebula.JobStepSourceBuilder.BuildInMemoryJobQueue<FirstJobStep>(jobId);

            await jobManager.StartJob(Tenant.Id, jobId);

            await queue.Enqueue(new FirstJobStep {Number = 1});
            var initialQueueLength = await queue.GetQueueLength();

            await jobManager.StopJob(Tenant.Id, jobId);
            var afterStopQueueLength = await queue.GetQueueLength();

            Assert.AreEqual(1, initialQueueLength);
            Assert.AreEqual(0, afterStopQueueLength);
        }

        [TestMethod]
        public async Task StopJob_StopStartedJob_CheckJobState()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);
            Nebula.RegisterJobProcessor(typeof(FirstJobProcessor), typeof(FirstJobStep));

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    QueueTypeName = QueueType.InMemory
                });

            var queue = Nebula.JobStepSourceBuilder.BuildInMemoryJobQueue<FirstJobStep>(jobId);
            await queue.Enqueue(new FirstJobStep {Number = 1});

            await jobManager.StartJob(Tenant.Id, jobId);
            await jobManager.StopJob(Tenant.Id, jobId);

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore.Load(Tenant.Id, jobId);

            Assert.IsNotNull(job);
            Assert.AreEqual(JobState.Stopped, job.Status.State);
        }

        private async Task AddJobToDb(JobState jobState)
        {
            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            await jobStore.AddOrUpdateDefinition(new JobData
            {
                TenantId = Tenant.Id,
                JobId = _jobId,
                Status = new JobStatusData {State = jobState},
                Configuration = new JobConfigurationData()
            });
        }
    }
}