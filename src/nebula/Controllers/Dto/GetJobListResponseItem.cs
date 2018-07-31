using System;

namespace Nebula.Controllers.Dto
{
    public class GetJobListResponseItem
    {
        public string JobId { get; set; }
        public string State { get; set; }
        public DateTime StateTime { get; set; }
        public bool IsCompleted { get; set; }
        public string JobDisplayName { get; set; }
        public DateTime CreationTime { get; set; }
    }
}