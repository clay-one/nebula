using System;
using System.Threading.Tasks;
using ComposerCore.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
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
        private bool _initialized;

        [TestInitialize]
        public void Initialize()
        {
            if (_initialized)
                return;

            NebulaContext.RegisterJobQueue( typeof(FirstJobQueue<>), nameof(FirstJobStep));
            NebulaContext.RegisterJobQueue(typeof(SecondJobQueue<>), nameof(SecondJobStep));
            _initialized = true;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task QueueTtpe_notDefined_ExceptionThrown()
        {
            var jobManager = NebulaContext.GetJobManager();

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
            var jobManager = NebulaContext.GetJobManager();

            await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = "notRegisteredQueue"
                });
        }

        [TestMethod]
        public async Task QueueTtpe_CreateCustome_Success()
        {
            //create sample job with custom queue
            var jobManager = NebulaContext.GetJobManager();
            var jobStore = NebulaContext.ComponentContext.GetComponent<IJobStore>();

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

            var jobData = await jobStore.LoadFromAnyTenant(jobId);
            var jobQueue =
                NebulaContext.GetJobQueue<IJobQueue<FirstJobStep>>(typeof(FirstJobStep), jobData.Configuration.QueueName);

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), jobQueue.GetType());
        }

        [TestMethod]
        public async Task QueueTtpe_DifferentJobs_DifferentQueues()
        {
            //create sample job with custom queue
            var jobManager = NebulaContext.GetJobManager();
            var jobStore = NebulaContext.ComponentContext.GetComponent<IJobStore>();

            var firstJob = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
                string.Empty, "first-job", nameof(FirstJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = nameof(FirstJobStep)
                });

            var secondJob = await jobManager.CreateNewJobOrUpdateDefinition<SecondJobStep>(
                string.Empty, "second-job", nameof(SecondJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueName = nameof(SecondJobStep)
                });

            var firstJobData = await jobStore.LoadFromAnyTenant(firstJob);
            var firstJobQueue =
                NebulaContext.GetJobQueue<IJobQueue<FirstJobStep>>(typeof(FirstJobStep), firstJobData.Configuration.QueueName) ;

            var secondJobData = await jobStore.LoadFromAnyTenant(secondJob);
            var secondJobQueue =
                NebulaContext.GetJobQueue<IJobQueue<SecondJobStep>>(typeof(SecondJobStep), secondJobData.Configuration.QueueName) ;

            Assert.AreEqual(typeof(FirstJobQueue<FirstJobStep>), firstJobQueue.GetType());
            Assert.AreEqual(typeof(SecondJobQueue<SecondJobStep>), secondJobQueue.GetType());
        }
    }
}