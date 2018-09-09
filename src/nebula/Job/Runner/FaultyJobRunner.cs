using System;
using System.Threading.Tasks;
using ComposerCore.Attributes;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Job.Runner
{
    [Contract]
    [Component]
    [ComponentCache(null)]
    internal class FaultyJobRunner : IJobRunner
    {
        private string _errorMessage;
        private Exception _exception;

        [ComponentPlug]
        public IJobStore JobStore { get; set; }
        
        public string TenantId { get; private set; }
        public string JobId { get; private set; }
        public bool IsProcessRunning => false;
        public bool IsProcessStopping => true;
        public bool IsProcessTerminated => true;
        
        public void Initialize(JobData jobData)
        {
            TenantId = jobData.TenantId;
            JobId = jobData.JobId;
            // TODO: Set error on the Job in store, and mark the job as completed.
        }

        public async Task<bool> CheckHealth()
        {
            // No need to re-start a faulty job, so returning "true". 
            // Just update the state and save the error, if not already done by some other worker.
            
            var status = await JobStore.LoadStatus(TenantId, JobId);
            if (status.State >= JobState.Completed)
                return true;

            if (!await JobStore.UpdateState(TenantId, JobId, status.State, JobState.Failed))
                return true;

            await JobStore.AddException(TenantId, JobId, BuildErrorData());
            
            return true;
        }

        public void StopRunner()
        {
            // Do nothing
        }

        public FaultyJobRunner SetFault(string errorMessage, Exception exception = null)
        {
            _errorMessage = errorMessage;
            _exception = exception;
            
            return this;
        }

        private JobStatusErrorData BuildErrorData()
        {
            // TODO: Add _exception information too

            var message = $"{_errorMessage}: {_exception.Message}";

            var exception = _exception;
            while (exception?.InnerException != null)
            {
                message += Environment.NewLine + _exception.InnerException.Message;
                exception = exception.InnerException;
            }

            return new JobStatusErrorData
            {
                ErrorMessage = message,
                Timestamp = DateTime.UtcNow.Ticks,
                StackTrace = _exception?.StackTrace
            };
        }
    }
}