namespace Nebula.Storage.Model
{
    public class JobConfigurationDefaultValues
    {
        public const int MinBatchSize = 1;
        public const int MaxBatchSize = 1000;

        public const int MinConcurrentBatchesPerWorker = 1;
        public const int MaxConcurrentBatchesPerWorker = 10000;

        public const double MinThrottledItemsPerSecond = 0.01d;

        public const int MinThrottledMaxBurstSize = 0;

        public const int MinIdleSecondsToCompletion = 10;

        public const int MinMaxBlockedSecondsPerCycle = 30;

        public const int MinMaxTargetQueueLength = 1;
    }
}