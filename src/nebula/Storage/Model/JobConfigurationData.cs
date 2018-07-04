using System;
using System.Collections.Generic;

namespace Nebula.Storage.Model
{
    public class JobConfigurationData
    {
        public int MaxBatchSize { get; set; }
        public int MaxConcurrentBatchesPerWorker { get; set; }
        public double? ThrottledItemsPerSecond { get; set; }
        public int? ThrottledMaxBurstSize { get; set; }
        
        public bool IsIndefinite { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? IdleSecondsToCompletion { get; set; }
        public int? MaxBlockedSecondsPerCycle { get; set; }
        public int? MaxTargetQueueLength { get;  set; }
        
        public string Parameters { get; set; }
        
        public List<string> PreprocessorJobIds { get; set; }
    }
}