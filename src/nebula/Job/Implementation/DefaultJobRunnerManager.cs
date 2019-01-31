using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ComposerCore;
using ComposerCore.Attributes;
using Hydrogen.General.Collections;
using Nebula.Job.Runner;
using Nebula.Queue;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Job.Implementation
{
    [Component]
    internal class DefaultJobRunnerManager : IJobRunnerManager
    {
        private readonly ConcurrentDictionary<string, IJobRunner> _runners;

        public DefaultJobRunnerManager()
        {
            _runners = new ConcurrentDictionary<string, IJobRunner>();
        }

        [ComponentPlug]
        public IJobStore JobStore { get; set; }

        [ComponentPlug]
        public IComposer Composer { get; set; }

        public async Task CheckStoreJobs()
        {
            var jobIds = await JobStore.LoadAllRunnableIdsFromAnyTenant();
            var runnerIds = _runners.Keys.ToList();

            var jobIdsToStart = jobIds.Except(runnerIds);

            foreach (var jobId in jobIdsToStart)
                await CheckHealthOrCreateRunner(jobId);
        }

        public async Task CheckHealthOfAllRunners()
        {
            foreach (var runner in _runners.Values.ToList())
                try
                {
                    if (runner.IsProcessTerminated)
                    {
                        if (await runner.CheckHealth())
                            RemoveRunnerFromList(runner.JobId);
                        else
                            await RestartRunner(runner.JobId);

                        continue;
                    }

                    if (await runner.CheckHealth())
                        continue;

                    await RestartRunner(runner.JobId);
                }
                catch (Exception)
                {
                    // TODO: Log
                }
        }

        public async Task CheckHealthOrCreateRunner(string jobId)
        {
            var runner = await GetOrCreateRunner(jobId);
            if (runner == null)
                return;

            if (!await runner.CheckHealth())
                await RestartRunner(jobId);
        }

        public void StopAllRunners()
        {
            foreach (var runner in _runners.Values.ToList())
                runner.StopRunner();
        }

        public bool IsJobRunnerActive(string jobId)
        {
            return _runners.TryGetValue(jobId, out var runner) && !runner.IsProcessTerminated;
        }

        public bool IsJobRunnerStarted(string jobId)
        {
            return _runners.TryGetValue(jobId, out var runner) && runner.IsProcessRunning;
        }

        public bool IsJobRunnerStoped(string jobId)
        {
            return _runners.TryGetValue(jobId, out var runner) && runner.IsProcessStopping;
        }

        private async Task<IJobRunner> GetOrCreateRunner(string jobId)
        {
            // Before loading data from store, check if the runner exists in memory
            if (_runners.TryGetValue(jobId, out var jobRunner))
                return jobRunner;

            var jobData = await JobStore.LoadFromAnyTenant(jobId);
            if (jobData.Status.State < JobState.InProgress || jobData.Status.State >= JobState.Completed)
                return null;

            foreach (var preJobId in jobData.Configuration.PreprocessorJobIds.EmptyIfNull())
                // Make sure preprocessor jobs are running before creating this runner
                await GetOrCreateRunner(preJobId);

            return _runners.GetOrAdd(jobId, s =>
            {
                try
                {
                    var newRunner = BuildNewJobRunner(jobData);
                    newRunner.Initialize(jobData);
                    return newRunner;
                }
                catch (Exception e)
                {
                    // TODO Log error details
                    var newRunner = Composer.GetComponent<FaultyJobRunner>()
                        .SetFault($"Fatal exception of type {e.GetType().Name} during runner initialization. " +
                                  $"Message: {e.Message}", e);

                    newRunner.Initialize(jobData);
                    return newRunner;
                }
            });
        }

        private IJobRunner BuildNewJobRunner(JobData jobData)
        {
            var stepType = Type.GetType(jobData.JobStepType);
            if (stepType == null)
                return Composer.GetComponent<FaultyJobRunner>()
                    .SetFault($"Type '{jobData.JobStepType}' could not be loaded.");

            if (!typeof(IJobStep).IsAssignableFrom(stepType))
                return Composer.GetComponent<FaultyJobRunner>()
                    .SetFault($"Type '{jobData.JobStepType}' should be a subclass of {nameof(IJobStep)}.");

            var contract = typeof(IJobRunner<>).MakeGenericType(stepType);
            if (!(Composer.GetComponent(contract) is IJobRunner jobRunner))
                return Composer.GetComponent<FaultyJobRunner>()
                    .SetFault($"Composer did not return an appropriate result for type '{jobData.JobStepType}'");

            return jobRunner;
        }

        private async Task RestartRunner(string jobId)
        {
            var runner = await GetOrCreateRunner(jobId);
            if (runner == null)
                return;

            if (runner.IsProcessStalled)
            {
                await KillRunner(jobId);
                await GetOrCreateRunner(jobId);
            }
        }

        private async Task KillRunner(string jobId)
        {
            var runner = await GetOrCreateRunner(jobId);
            if (runner == null)
                return;

            _runners.TryRemove(jobId, out var value);
        }

        private void RemoveRunnerFromList(string jobId)
        {
            _runners.TryRemove(jobId, out var _);
        }
    }
}