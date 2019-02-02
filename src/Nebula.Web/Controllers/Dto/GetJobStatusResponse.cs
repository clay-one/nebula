using System;
using System.Collections.Generic;

namespace Nebula.Web.Controllers.Dto
{
    public class GetJobStatusResponse
    {
        public string JobId { get; set; }
        public string JobDisplayName { get; set; }
        public string State { get; set; }
        public DateTime StateTime { get; set; }
        public bool IsCompleted { get; set; }
        public long QueueLength { get; set; }

        public DateTime LastActivityTime { get; set; }
        public DateTime LastProcessTime { get; set; }
        public DateTime LastHealthCheckTime { get; set; }

        public long ItemsProcessed { get; set; }
        public long ItemsFailed { get; set; }
        public long ItemsRequeued { get; set; }
        public long ItemsGeneratedForTargetQueue { get; set; }
        public long EstimatedTotalItems { get; set; }
        public long ProcessingTimeTakenMillis { get; set; }

        public long ExceptionCount { get; set; }
        public DateTime? LastExceptionTime { get; set; }
        public DateTime? LastFailTime { get; set; }
        public string[] LastFailures { get; set; }

        public DateTime CreationTime { get; set; }
        public List<string> PreprocessorJobIds { get; set; }
    }
}