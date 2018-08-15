using System.Configuration;
using ComposerCore.Attributes;
using MongoDB.Driver;
using Nebula.Storage.Model;

namespace Nebula.Connection.Implementation
{
    [Component]
    internal class DefaultJobMongoManager : IJobMongoManager
    {
        [ComponentPlug]
        public NebulaContext NebulaContext { get; set; }

        public IMongoDatabase Database { get; private set; }

        public IMongoCollection<JobData> Jobs { get; set; }
        public void DeleteTenantData(string tenantId)
        {
            Jobs.DeleteMany(jd => jd.TenantId == tenantId);

        }

        [OnCompositionComplete]
        public void OnCompositionComplete()
        {
            var connectionString = NebulaContext.MongoConnectionString;

            var mongoUrl = MongoUrl.Create(connectionString);
            var databaseName = mongoUrl.DatabaseName;

            var client = new MongoClient(mongoUrl);
            Database = client.GetDatabase(databaseName);
            Jobs = Database.GetCollection<JobData>(nameof(JobData));

        }

    }
}