using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;
using Test.SampleJob;
using Test.SampleJob.FirstJob;

namespace Test
{
    [TestClass]
    public class PurgeQueueTests : TestClassBase
    {
        [TestMethod]
        public async Task PurgeQueue_CustomeJobQueueDefinedInJob_shouldBeEmpty()
        {
            //create sample job with custom queue
            var jobManager = Nebula.GetJobManager();
            var jobStore = Nebula.ComponentContext.GetComponent<IJobStore>();

            Nebula.RegisterJobQueue(typeof(FirstJobQueue<FirstJobStep>), QueueTypes.FirstJobQueue);

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
                Nebula.JobStepSourceBuilder.BuildJobStepSource<FirstJobStep>(jobData.Configuration.QueueTypeName,
                    jobData.JobId) as IJobQueue<FirstJobStep>;

            await jobQueue.Enqueue(new FirstJobStep());
            await jobQueue.PurgeQueueContents();
            var queueLeghnt = await jobQueue.GetQueueLength();

            Assert.AreEqual(0, queueLeghnt);
        }
    }
}