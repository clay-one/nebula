using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Job;
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
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QueueTtpe_notDefined_ExceptionThrown()
        {
            var jobManager = Composer.GetComponent<IJobManager>();

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
        public async Task QueueTtpe_CreateCustome_Success()
        {
            //create sample job with custom queue
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();

            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueDescriptor = new FirstQueueDescriptor()
                });

            var jobData = await jobStore.LoadFromAnyTenant(jobId);
            var jobQueue = jobData.Configuration.QueueDescriptor.GetQueue<FirstJobStep>(Composer);
            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), jobQueue.GetType());
        }

        [TestMethod]
        public async Task QueueTtpe_DifferentJobs_DifferentQueues()
        {
            //create sample job with custom queue
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();

            var firstJob = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "first-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueDescriptor = new FirstQueueDescriptor()
                });

            var secondJob = await jobManager.CreateNewJobOrUpdateDefinition<SecondJobStep>(
                string.Empty, "second-job", nameof(SecondJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueDescriptor = new SecondQueueDescriptor()
                });

            var firstJobData = await jobStore.LoadFromAnyTenant(firstJob);
            var firstJobQueue = firstJobData.Configuration.QueueDescriptor.GetQueue<FirstJobStep>(Composer);

            var secondJobData = await jobStore.LoadFromAnyTenant(secondJob);
            var secondJobQueue = secondJobData.Configuration.QueueDescriptor.GetQueue<SecondJobStep>(Composer);

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), firstJobQueue.GetType());
            Assert.AreEqual(typeof(SecondJobQueue<SecondJobStep>), secondJobQueue.GetType());
        }
    }
}