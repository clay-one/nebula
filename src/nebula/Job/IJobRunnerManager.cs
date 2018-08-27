using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ComposerCore.Attributes;

[assembly: InternalsVisibleTo("Test")]

namespace Nebula.Job
{
    [Contract]
    internal interface IJobRunnerManager
    {
        Task CheckStoreJobs();
        Task CheckHealthOfAllRunners();

        Task CheckHealthOrCreateRunner(string jobId);
        void StopAllRunners();

        bool IsJobRunnerActive(string jobId);

        bool IsJobRunnerStarted(string jobId);
    }
}