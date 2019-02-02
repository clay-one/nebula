using System.Collections.Generic;
using System.Linq;
using Hydrogen.General.Collections;

namespace Nebula.Queue
{
    public class JobProcessingResult
    {
        public long ItemsFailed { get; set; }
        public long ItemsRequeued { get; set; }
        public long ItemsGeneratedForTargetQueue { get; set; }
        
        public string[] FailureMessages { get; set; }

        public static JobProcessingResult Combine(IEnumerable<JobProcessingResult> results)
        {
            var resultArray = results as JobProcessingResult[] ?? 
                              results.ToArray();

            return new JobProcessingResult
            {
                ItemsFailed = resultArray.Sum(r => r.ItemsFailed),
                ItemsRequeued = resultArray.Sum(r => r.ItemsRequeued),
                ItemsGeneratedForTargetQueue = resultArray.Sum(r => r.ItemsGeneratedForTargetQueue),
                FailureMessages = resultArray.SelectMany(r => r.FailureMessages.EmptyIfNull()).ToArray()
            };
        }
    }
}