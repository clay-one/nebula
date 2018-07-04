using System;

namespace Nebula.Storage.Model
{
    public class JobStatusUpdateData
    {
        public DateTime LastIterationStartTime { get; set; }
        public DateTime LastDequeueAttemptTime { get; set; }
        public DateTime LastProcessStartTime { get; set; }
        public DateTime LastProcessFinishTime { get; set; }
        public DateTime LastHealthCheckTime { get; set; }
        
        public long ItemsProcessedDelta { get; set; }
        public long ItemsRequeuedDelta { get; set; }
        public long ItemsGeneratedForTargetQueueDelta { get; set; }
        public long ProcessingTimeTakenMillisDelta { get; set; }
        
        public long ItemsFailedDelta { get; set; }
        public DateTime? LastFailTime { get; set; }
        public JobStatusErrorData[] LastFailures { get; set; }
    }
}