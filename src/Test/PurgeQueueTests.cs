using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        //[TestMethod]
        //public async Task PurgeQueue_CustomeJobQueueDefinedInJob_shouldBeEmpty()
        //{
        //    //create sample job with custom queue
        //    var jobManager = Composer.GetComponent<IJobManager>();
        //    var jobStore = Composer.GetComponent<IJobStore>();

        //    var queueName = nameof(FirstJobStep);
        //    Composer.Register(typeof(IJobQueue<>), queueName, typeof(FirstJobQueue<>));

        //    var jobId = await jobManager.CreateNewJobOrUpdateDefinition<FirstJobStep>(
        //        string.Empty, "sample-job", nameof(FirstJobStep), new JobConfigurationData
        //        {
        //            MaxBatchSize = 100,
        //            MaxConcurrentBatchesPerWorker = 5,
        //            IsIndefinite = true,
        //            MaxBlockedSecondsPerCycle = 300,
        //            QueueName = queueName
        //        });

        //    var jobData = await jobStore.LoadFromAnyTenant(jobId);
        //    var jobQueue = GetJobQueue(typeof(FirstJobStep), jobData.Configuration.QueueName) as IJobQueue<FirstJobStep>;

        //    await jobQueue.Enqueue(new FirstJobStep(), jobData.JobId);
        //    await jobQueue.PurgeQueueContents(jobData.JobId);
        //    var queueLeghnt = await jobQueue.GetQueueLength();

        //    Assert.AreEqual(0, queueLeghnt);
        //}

        [TestMethod]
        public async Task PurgeQueue_CustomeJobQueueDefinedInJob_shouldBeEmpty()
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
            var jobQueue = GetJobQueue(typeof(FirstJobStep), jobData.Configuration.QueueName) as IJobQueue<FirstJobStep>;

            await jobQueue.Enqueue(new FirstJobStep(), jobData.JobId);
            await jobQueue.PurgeQueueContents(jobData.JobId);
            var queueLeghnt = await jobQueue.GetQueueLength();

            Assert.AreEqual(0, queueLeghnt);
        }
    }
}