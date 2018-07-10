using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Nebula.Storage.Model
{
    public class JobData
    {
        [BsonId]
        public string JobId { get; set; }
        public string TenantId { get; set; }
        public string JobDisplayName { get; set; }
        public string JobStepType { get; set; }

        public DateTime CreationTime { get; set; }
        public string CreatedBy { get; set; }
        
        public JobConfigurationData Configuration { get; set; }
        public JobStatusData Status { get; set; }
    }
}