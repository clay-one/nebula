using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;
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
            var jobManager = NebulaContext.GetJobManager();
            var jobStore = NebulaContext.ComponentContext.GetComponent<IJobStore>();

            var queueName = nameof(FirstJobStep);
            NebulaContext.RegisterJobQueue(typeof(FirstJobQueue<>), queueName);

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
                NebulaContext.GetJobQueue<IJobQueue<FirstJobStep>>(typeof(FirstJobStep),
                    jobData.Configuration.QueueName);

            await jobQueue.Enqueue(new FirstJobStep(), jobData.JobId);
            await jobQueue.PurgeQueueContents(jobData.JobId);
            var queueLeghnt = await jobQueue.GetQueueLength();

            Assert.AreEqual(0, queueLeghnt);
        }
    }
}