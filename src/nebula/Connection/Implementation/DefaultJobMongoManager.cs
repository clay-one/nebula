using ComposerCore.Attributes;
using MongoDB.Driver;
using Nebula.Storage.Model;

namespace Nebula.Connection.Implementation
{
    [Component]
    internal class DefaultJobMongoManager : IJobMongoManager
    {
        [ConfigurationPoint("mongo.clientSettings")]
        public MongoClientSettings ClientSettings { get; set; }

        [ConfigurationPoint("mongo.databaseName")]
        public string DatabaseName { get; set; }

        [ConfigurationPoint("mongo.databaseSettings")]
        public MongoDatabaseSettings DatabaseSettings { get; set; }

        [ConfigurationPoint("mongo.authenticationSettings", false)]
        public MongoAuthenticationSettings AuthenticationSettings { get; set; }

        public IMongoDatabase Database { get; private set; }

        public IMongoCollection<JobData> Jobs { get; set; }
        public void DeleteTenantData(string tenantId)
        {
            Jobs.DeleteMany(jd => jd.TenantId == tenantId);

        }

        [OnCompositionComplete]
        public void OnCompositionComplete()
        {
            if (AuthenticationSettings != null)
            {
                ClientSettings.Credential = MongoCredential.CreateCredential(AuthenticationSettings.AuthenticatorDbName,
                    AuthenticationSettings.Username, AuthenticationSettings.Password);
            }
            
            var client = new MongoClient(ClientSettings);
            Database = client.GetDatabase(DatabaseName, DatabaseSettings);
            Jobs = Database.GetCollection<JobData>(nameof(JobData));

        }
    }
}