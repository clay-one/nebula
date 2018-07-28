using ComposerCore.Attributes;
using MongoDB.Driver;
using Nebula.Storage.Model;

namespace Nebula.Connection
{
    [Contract]
    internal interface IJobMongoManager
    {
        IMongoDatabase Database { get; }

        IMongoCollection<JobData> Jobs { get; set; }

        void DeleteTenantData(string tenantId);
    }
}