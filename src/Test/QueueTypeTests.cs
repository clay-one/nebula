using System;
using System.Threading.Tasks;
using ComposerCore.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;
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
        [ExpectedException(typeof(CompositionException))]
        public async Task QueueTtpe_notRegisteredQueue_ExceptionThrown()
        {
            //create sample job with custom queue
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();

            var queueName = nameof(FirstJobStep);

            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = queueName
                });
        }

        [TestMethod]
        public async Task QueueTtpe_CreateCustome_Success()
        {
            //create sample job with custom queue
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();

            var queueName = nameof(FirstJobStep);
            Composer.Register(typeof(IJobQueue<>), queueName, typeof(FirstJobQueue<>));

            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = queueName
                });

            var jobData = await jobStore.LoadFromAnyTenant(jobId);
            var jobQueue =
                GetJobQueue(typeof(FirstJobStep), jobData.Configuration.QueueName) as IJobQueue<FirstJobStep>;

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), jobQueue.GetType());
        }

        [TestMethod]
        public async Task QueueTtpe_DifferentJobs_DifferentQueues()
        {
            //create sample job with custom queue
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();

            var firstQueueName = nameof(FirstJobStep);
            var secondQueueName = nameof(SecondJobStep);

            Composer.Register(typeof(IJobQueue<>), firstQueueName, typeof(FirstJobQueue<>));
            Composer.Register(typeof(IJobQueue<>), secondQueueName, typeof(SecondJobQueue<>));

            var firstJob = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "first-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = firstQueueName
                });

            var secondJob = await jobManager.CreateNewJobOrUpdateDefinition<SecondJobStep>(
                string.Empty, "second-job", nameof(SecondJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = secondQueueName
                });

            var firstJobData = await jobStore.LoadFromAnyTenant(firstJob);
            var firstJobQueue =
                GetJobQueue(typeof(FirstJobStep), firstJobData.Configuration.QueueName) as IJobQueue<FirstJobStep>;

            var secondJobData = await jobStore.LoadFromAnyTenant(secondJob);
            var secondJobQueue =
                GetJobQueue(typeof(SecondJobStep), secondJobData.Configuration.QueueName) as IJobQueue<SecondJobStep>;

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), firstJobQueue.GetType());
            Assert.AreEqual(typeof(SecondJobQueue<SecondJobStep>), secondJobQueue.GetType());
        }
    }
}