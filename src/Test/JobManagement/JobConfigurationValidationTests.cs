using System;
using System.Threading.Tasks;
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
    public class JobConfigurationValidationTests : TestClassBase
    {
        private readonly NullTenant _tenant = new NullTenant();

        [TestInitialize]
        public void Initialize()
        {
            _tenant.GetCurrentTenant();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_NullConfiguration_ExceptionThrown()
        {
            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id);
        }

        [TestMethod]
        public async Task CreateJob_MinBatchSize_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBatchSize = JobConfigurationDefaultValue.MinBatchSize - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBatchSize >= JobConfigurationDefaultValue.MinBatchSize);
        }

        [TestMethod]
        public async Task CreateJob_MaxBatchSizeRange_LessTahnMax()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBatchSize = JobConfigurationDefaultValue.MaxBatchSize + 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBatchSize <= JobConfigurationDefaultValue.MaxBatchSize);
        }

        [TestMethod]
        public async Task CreateJob_MinConcurrentBatchesPerWorker_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxConcurrentBatchesPerWorker = JobConfigurationDefaultValue.MinConcurrentBatchesPerWorker - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxConcurrentBatchesPerWorker >=
                          JobConfigurationDefaultValue.MinConcurrentBatchesPerWorker);
        }

        [TestMethod]
        public async Task CreateJob_MaxConcurrentBatchesPerWorker_LessThanMax()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxConcurrentBatchesPerWorker = JobConfigurationDefaultValue.MaxConcurrentBatchesPerWorker + 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxConcurrentBatchesPerWorker <=
                          JobConfigurationDefaultValue.MaxConcurrentBatchesPerWorker);
        }

        [TestMethod]
        public async Task CreateJob_DefaultThrottledItemsPerSecond_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    ThrottledItemsPerSecond = JobConfigurationDefaultValue.MinThrottledItemsPerSecond - 0.0001,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.ThrottledItemsPerSecond >=
                          JobConfigurationDefaultValue.MinThrottledItemsPerSecond);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_ZeroThrottledItemsPerSecond_ExceptionThrown()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
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
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    ThrottledMaxBurstSize = JobConfigurationDefaultValue.MinThrottledMaxBurstSize,
                    QueueTypeName = QueueType.InMemory
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateJob_ExpiredJob_ExceptionThrown()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
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
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    IdleSecondsToCompletion = JobConfigurationDefaultValue.MinIdleSecondsToCompletion - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.IdleSecondsToCompletion >=
                          JobConfigurationDefaultValue.MinIdleSecondsToCompletion);
        }

        [TestMethod]
        public async Task CreateJob_MaxBlockedSecondsPerCycle_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxBlockedSecondsPerCycle = JobConfigurationDefaultValue.MinMaxBlockedSecondsPerCycle - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxBlockedSecondsPerCycle >=
                          JobConfigurationDefaultValue.MinMaxBlockedSecondsPerCycle);
        }

        [TestMethod]
        public async Task CreateJob_MaxTargetQueueLength_MoreThanMin()
        {
            Nebula.RegisterJobQueue(typeof(InMemoryJobQueue<FirstJobStep>), QueueType.InMemory);

            var jobManager = Nebula.GetJobManager();
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(_tenant.Id,
                configuration: new JobConfigurationData
                {
                    MaxTargetQueueLength = JobConfigurationDefaultValue.MinMaxTargetQueueLength - 1,
                    QueueTypeName = QueueType.InMemory
                });

            var jobStore = Nebula.ComponentContext.GetComponent(typeof(IJobStore)) as IJobStore;
            var job = await jobStore?.Load(_tenant.Id, jobId);

            Assert.IsTrue(job.Configuration.MaxTargetQueueLength >=
                          JobConfigurationDefaultValue.MinMaxTargetQueueLength);
        }
    }
}