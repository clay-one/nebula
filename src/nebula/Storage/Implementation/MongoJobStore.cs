using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Hydrogen.General.Utils;
using MongoDB.Driver;
using Nebula.Connection;
using Nebula.Storage.Model;

namespace Nebula.Storage.Implementation
{
    [Component]
    internal class MongoJobStore : IJobStore
    {
        private const int NumberOfExceptionsToKeep = 20;
        private const int NumberOfFailuresToKeep = 20;
        
        [ComponentPlug]
        public IJobMongoManager Mongo { get; set; }

        public async Task<List<JobData>> LoadAll(string tenantId)
        {
            var cursor = await Mongo.Jobs.FindAsync(
                Builders<JobData>.Filter.Eq(jd => jd.TenantId, tenantId)
            );
            return await cursor.ToListAsync();
        }

        public async Task<JobData> Load(string tenantId, string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentNullException(nameof(jobId));

            var cursor = await Mongo.Jobs.FindAsync(jd => jd.JobId == jobId && jd.TenantId == tenantId);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<JobStatusData> LoadStatus(string tenantId, string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentNullException(nameof(jobId));

            var cursor = await Mongo.Jobs.FindAsync(
                Builders<JobData>.Filter.And(
                    Builders<JobData>.Filter.Eq(jd => jd.TenantId, tenantId),
                    Builders<JobData>.Filter.Eq(jd => jd.JobId, jobId)
                ),
                new FindOptions<JobData, JobStatusData>
                {
                    Projection = Builders<JobData>.Projection.Expression(jd => jd.Status)
                }
            );;
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<JobData> LoadFromAnyTenant(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentNullException(nameof(jobId));

            var cursor = await Mongo.Jobs.FindAsync(jd => jd.JobId == jobId);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<List<string>> LoadAllRunnableIdsFromAnyTenant()
        {
            var cursor = await Mongo.Jobs.FindAsync(
                Builders<JobData>.Filter.And(
                    Builders<JobData>.Filter.Gte(jd => jd.Status.State, JobState.InProgress),
                    Builders<JobData>.Filter.Lt(jd => jd.Status.State, JobState.Completed)),
                new FindOptions<JobData, string>
                {
                    Projection = Builders<JobData>.Projection.Expression(jd => jd.JobId)
                }
            );
            return await cursor.ToListAsync();
        }

        public async Task<JobData> AddOrUpdateDefinition(JobData jobData)
        {
            var filter = Builders<JobData>.Filter.And(
                Builders<JobData>.Filter.Eq(jd => jd.TenantId, jobData.TenantId),
                Builders<JobData>.Filter.Eq(jd => jd.JobId, jobData.JobId)
            );

            var update = Builders<JobData>.Update
                .SetOnInsert(jd => jd.JobId, jobData.JobId)
                .SetOnInsert(jd => jd.TenantId, jobData.TenantId)
                .SetOnInsert(jd => jd.JobDisplayName, jobData.JobDisplayName)
                .SetOnInsert(jd => jd.JobStepType, jobData.JobStepType)
                .SetOnInsert(jd => jd.CreatedBy, "unknown")
                .SetOnInsert(jd => jd.CreationTime, DateTime.UtcNow)
                .SetOnInsert(jd => jd.Status, jobData.Status)
                .Set(jd => jd.Configuration, jobData.Configuration);

            return await Mongo.Jobs.FindOneAndUpdateAsync(filter, update,
                new FindOneAndUpdateOptions<JobData> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
        }

        public async Task<bool> UpdateState(string tenantId, string jobId, JobState? expectedState, JobState newState)
        {
            var update = Builders<JobData>.Update
                .Set(jd => jd.Status.State, newState)
                .Set(jd => jd.Status.StateTime, DateTime.UtcNow);

            if (expectedState.HasValue)
                return (await Mongo.Jobs.UpdateOneAsync(jd =>
                               jd.TenantId == tenantId && 
                               jd.JobId == jobId &&
                               jd.Status.State == expectedState.Value,
                           update)).ModifiedCount > 0;

            return (await Mongo.Jobs.UpdateOneAsync(jd => jd.TenantId == tenantId && jd.JobId == jobId, update))
                   .ModifiedCount > 0;
        }

        public async Task UpdateStatus(string tenantId, string jobId, JobStatusUpdateData change)
        {
            var filter = Builders<JobData>.Filter.And(
                Builders<JobData>.Filter.Eq(jd => jd.TenantId, tenantId),
                Builders<JobData>.Filter.Eq(jd => jd.JobId, jobId));

            var update = Builders<JobData>.Update
                .Max(jd => jd.Status.LastIterationStartTime, change.LastIterationStartTime)
                .Max(jd => jd.Status.LastDequeueAttemptTime, change.LastDequeueAttemptTime)
                .Max(jd => jd.Status.LastProcessStartTime, change.LastProcessStartTime)
                .Max(jd => jd.Status.LastProcessFinishTime, change.LastProcessFinishTime)
                .Max(jd => jd.Status.LastHealthCheckTime, change.LastHealthCheckTime)
                .Inc(jd => jd.Status.ItemsProcessed, change.ItemsProcessedDelta)
                .Inc(jd => jd.Status.ItemsRequeued, change.ItemsRequeuedDelta)
                .Inc(jd => jd.Status.ItemsGeneratedForTargetQueue, change.ItemsGeneratedForTargetQueueDelta)
                .Inc(jd => jd.Status.ProcessingTimeTakenMillis, change.ProcessingTimeTakenMillisDelta)
                .Inc(jd => jd.Status.ItemsFailed, change.ItemsFailedDelta);

            if (change.LastFailTime.HasValue)
                update = update.Max(jd => jd.Status.LastFailTime, change.LastFailTime.Value);

            if (change.LastFailures != null && change.LastFailures.Length > 0)
                update = update.PushEach(jd => jd.Status.LastFailures, change.LastFailures, -NumberOfFailuresToKeep);
            
            await Mongo.Jobs.UpdateOneAsync(filter, update);
        }

        public async Task AddException(string tenantId, string jobId, JobStatusErrorData exceptionData)
        {
            var filter = Builders<JobData>.Filter.And(
                Builders<JobData>.Filter.Eq(jd => jd.TenantId, tenantId),
                Builders<JobData>.Filter.Eq(jd => jd.JobId, jobId));

            var update = Builders<JobData>.Update
                .Inc(jd => jd.Status.ExceptionCount, 1L)
                .Max(jd => jd.Status.LastExceptionTime, DateTime.UtcNow)
                .PushEach(jd => jd.Status.LastExceptions, exceptionData.Yield(), -NumberOfExceptionsToKeep);

            await Mongo.Jobs.UpdateOneAsync(filter, update);
        }

        public async Task<bool> AddPredecessor(string tenantId, string jobId, string predecessorJobId)
        {
            var update = Builders<JobData>.Update
                .Push(jd => jd.Configuration.PreprocessorJobIds, predecessorJobId);

            return (await Mongo.Jobs.UpdateOneAsync(jd =>
                           jd.TenantId == tenantId &&
                           jd.JobId == jobId &&
                           jd.Status.State < JobState.InProgress,
                       update)).ModifiedCount > 0;
        }
    }
}