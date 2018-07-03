using System;

namespace Nebula.Storage.Model
{
    public class JobStatusData
    {
        public JobState State { get; set; }
        public DateTime StateTime { get; set; }
        
        public DateTime LastIterationStartTime { get; set; }
        public DateTime LastDequeueAttemptTime { get; set; }
        public DateTime LastProcessStartTime { get; set; }
        public DateTime LastProcessFinishTime { get; set; }
        public DateTime LastHealthCheckTime { get; set; }
        
        public long ItemsProcessed { get; set; }
        public long ItemsRequeued { get; set; }
        public long ItemsGeneratedForTargetQueue { get; set; }
        public long EstimatedTotalItems { get; set; }
        public long ProcessingTimeTakenMillis { get; set; }
        
        public long ItemsFailed { get; set; }
        public DateTime? LastFailTime { get; set; }
        public JobStatusErrorData[] LastFailures { get; set; }

        public long ExceptionCount { get; set; }
        public DateTime? LastExceptionTime { get; set; }
        public JobStatusErrorData[] LastExceptions { get; set; }
        
    }
}