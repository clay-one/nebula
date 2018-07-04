﻿using System.Threading.Tasks;
using ComposerCore.Attributes;

namespace Nebula.Job
{
    [Contract]
    public interface IJobRunnerManager
    {
        Task CheckStoreJobs();
        Task CheckHealthOfAllRunners();
        
        Task CheckHealthOrCreateRunner(string jobId);
        void StopAllRunners();
        
        bool IsJobRunnerActive(string jobId);
    }
}