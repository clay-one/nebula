namespace Nebula.Storage.Model
{
    public enum JobState : byte
    {
        Initializing = 1,
        
        // Anything above this, means the job runner should be started
        InProgress = 100,
        Draining = 140,
        Paused = 150,
        
        // Anything above this, means the job runner should be terminated
        Completed = 200,
        Failed = 230,
        Expired = 240,
        Stopped = 250,
    }
}