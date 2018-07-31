using System;
using System.Threading.Tasks;
using ComposerCore.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob;
using Test.SampleJob.FirstJob;
using Test.SampleJob.SecondJob;

namespace Test
{
    [TestClass]
    public class QueueTypeTests : TestClassBase
    {
        private bool _initialized;

        [TestInitialize]
        public void Initialize()
        {
            if (_initialized)
                return;

            Nebula.RegisterJobQueue(typeof(FirstJobQueue<FirstJobStep>), nameof(FirstJobStep));
            Nebula.RegisterJobQueue(typeof(SecondJobQueue<SecondJobStep>), nameof(SecondJobStep));
            _initialized = true;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QueueType_notDefined_ExceptionThrown()
        {
            var jobManager = Nebula.GetJobManager();

            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300
                });
        }

        [TestMethod]
        [ExpectedException(typeof(CompositionException))]
        public async Task QueueType_notRegisteredQueue_ExceptionThrown()
        {
            //create sample job with custom queue
            var jobManager = Nebula.GetJobManager();

            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueTypeName = "notRegisteredQueue"
                });
        }

        [TestMethod]
        public async Task QueueType_CreateCustome_Success()
        {
            //create sample job with custom queue
            Nebula.RegisterJobQueue(typeof(FirstJobQueue<>), QueueTypes.FirstJobQueue);

            var jobManager = Nebula.GetJobManager();
            var jobStore = Nebula.ComponentContext.GetComponent<IJobStore>();

            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueTypeName = QueueTypes.FirstJobQueue
                });

            var jobData = await jobStore.LoadFromAnyTenant(jobId);
            var jobQueue =
                Nebula.GetJobQueue<FirstJobStep>(jobData.Configuration.QueueTypeName);

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), jobQueue.GetType());
        }

        [TestMethod]
        public async Task QueueType_DifferentJobs_DifferentQueues()
        {
            //create sample job with custom queue
            var jobManager = Nebula.GetJobManager();
            var jobStore = Nebula.ComponentContext.GetComponent<IJobStore>();

            Nebula.RegisterJobQueue(typeof(FirstJobQueue<>), QueueTypes.FirstJobQueue);
            Nebula.RegisterJobQueue(typeof(SecondJobQueue<>), QueueTypes.SecondJobQueue);

            var firstJob = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "first-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueTypeName = QueueTypes.FirstJobQueue 
                });

            var secondJob = await jobManager.CreateNewJobOrUpdateDefinition<SecondJobStep>(
                string.Empty, "second-job", nameof(SecondJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueTypeName = QueueTypes.SecondJobQueue
                });

            var firstJobData = await jobStore.LoadFromAnyTenant(firstJob);
            var firstJobQueue =
                Nebula.GetJobQueue<FirstJobStep>(firstJobData.Configuration.QueueTypeName);

            var secondJobData = await jobStore.LoadFromAnyTenant(secondJob);
            var secondJobQueue =
                Nebula.GetJobQueue<SecondJobStep>(secondJobData.Configuration.QueueTypeName);

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), firstJobQueue.GetType());
            Assert.AreEqual(typeof(SecondJobQueue<SecondJobStep>), secondJobQueue.GetType());
        }
    }
}