using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using ComposerCore.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nebula.Job;
using Nebula.Queue;
using Nebula.Queue.Implementation;
using Nebula.Storage;
using Nebula.Storage.Model;

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

            await jobManager.CreateNewJobOrUpdateDefinition<SampleJobStep>(
                string.Empty, "sample-job", nameof(SampleJobStep), new JobConfigurationData
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
            var jobManager = Composer.GetComponent<IJobManager>();
            var jobStore = Composer.GetComponent<IJobStore>();
           
            var jobId = await jobManager.CreateNewJobOrUpdateDefinition<SampleJobStep>(
                string.Empty, "sample-job", nameof(SampleJobStep), new JobConfigurationData
                {
                    MaxBatchSize = 100,
                    MaxConcurrentBatchesPerWorker = 5,
                    IsIndefinite = true,
                    MaxBlockedSecondsPerCycle = 300,
                    QueueDescriptor = new SampleQueueDescriptor()
                });

            var jobData = await jobStore.LoadFromAnyTenant(jobId);
            var jobQueue = jobData.Configuration.QueueDescriptor.GetQueue<SampleJobStep>(Composer);
            Assert.AreEqual(typeof(SampleJobQueue<SampleJobStep>), jobQueue.GetType());
        }
        
        #region entities

        public class SampleJobStep : IJobStep
        {
            public int Number { get; set; }
        }

        [Component]
        [Contract]
        public class SampleQueueDescriptor : QueueDescriptor<SampleJobStep>
        {
            public SampleQueueDescriptor() : base(typeof(SampleJobQueue<SampleJobStep>).AssemblyQualifiedName)
            {
            }
        }

        [Component]
        [IgnoredOnAssemblyRegistration]
        public class SampleJobQueue<TItem> : IJobQueue<TItem> where TItem : IJobStep
        {
            public Task EnsureJobQueueExists(string jobId = null)
            {
                return Task.CompletedTask;
            }

            public Task<long> GetQueueLength(string jobId = null)
            {
                throw new NotImplementedException();
            }

            public Task PurgeQueueContents(string jobId = null)
            {
                throw new NotImplementedException();
            }

            public Task Enqueue(TItem item, string jobId = null)
            {
                throw new NotImplementedException();
            }

            public Task EnqueueBatch(IEnumerable<TItem> items, string jobId = null)
            {
                throw new NotImplementedException();
            }

            public Task<TItem> Dequeue(string jobId = null)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<TItem>> DequeueBatch(int maxBatchSize, string jobId = null)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}