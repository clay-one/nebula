using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob.FirstJob;

namespace Test.JobManagement
{
    [TestClass]
    public class JobConfigurationValidationTests : TestClassBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_NullConfiguration_ExceptionThrown()
        {
            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id);
        }

        [TestMethod]
        public async Task CreateJob_MinBatchSize_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBatchSize = JobConfigurationDefaultValues.MinBatchSize - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBatchSize >= JobConfigurationDefaultValues.MinBatchSize);
        }

        [TestMethod]
        public async Task CreateJob_MaxBatchSizeRange_LessTahnMax()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBatchSize = JobConfigurationDefaultValues.MaxBatchSize + 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBatchSize <= JobConfigurationDefaultValues.MaxBatchSize);
        }

        [TestMethod]
        public async Task CreateJob_MinConcurrentBatchesPerWorker_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxConcurrentBatchesPerWorker = JobConfigurationDefaultValues.MinConcurrentBatchesPerWorker - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxConcurrentBatchesPerWorker >=
                          JobConfigurationDefaultValues.MinConcurrentBatchesPerWorker);
        }

        [TestMethod]
        public async Task CreateJob_MaxConcurrentBatchesPerWorker_LessThanMax()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxConcurrentBatchesPerWorker = JobConfigurationDefaultValues.MaxConcurrentBatchesPerWorker + 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxConcurrentBatchesPerWorker <=
                          JobConfigurationDefaultValues.MaxConcurrentBatchesPerWorker);
        }

        [TestMethod]
        public async Task CreateJob_DefaultThrottledItemsPerSecond_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    ThrottledItemsPerSecond = JobConfigurationDefaultValues.MinThrottledItemsPerSecond - 0.0001,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.ThrottledItemsPerSecond >=
                          JobConfigurationDefaultValues.MinThrottledItemsPerSecond);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_ZeroThrottledItemsPerSecond_ExceptionThrown()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    ThrottledItemsPerSecond = 0,
                    QueueTypeName = QueueType.InMemory
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_ZeroThrottledMaxBurstSize_ExceptionThrown()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    ThrottledMaxBurstSize = JobConfigurationDefaultValues.MinThrottledMaxBurstSize,
                    QueueTypeName = QueueType.InMemory
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_ExpiredJob_ExceptionThrown()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    ExpiresAt = DateTime.Now.AddDays(-1),
                    QueueTypeName = QueueType.InMemory
                });
        }

        [TestMethod]
        public async Task CreateJob_IdleSecondsToCompletion_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    IdleSecondsToCompletion = JobConfigurationDefaultValues.MinIdleSecondsToCompletion - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.IdleSecondsToCompletion >=
                          JobConfigurationDefaultValues.MinIdleSecondsToCompletion);
        }

        [TestMethod]
        public async Task CreateJob_MaxBlockedSecondsPerCycle_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBlockedSecondsPerCycle = JobConfigurationDefaultValues.MinMaxBlockedSecondsPerCycle - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBlockedSecondsPerCycle >=
                          JobConfigurationDefaultValues.MinMaxBlockedSecondsPerCycle);
        }

        [TestMethod]
        public async Task CreateJob_MaxTargetQueueLength_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(Tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxTargetQueueLength = JobConfigurationDefaultValues.MinMaxTargetQueueLength - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(Tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxTargetQueueLength >=
                          JobConfigurationDefaultValues.MinMaxTargetQueueLength);
        }
    }
}