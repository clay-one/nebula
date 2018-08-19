namespace Nebula.Storage.Model
{
    public class JobStatusErrorData
    {
        public long Timestamp { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }
}