using System.Linq;
using System.Threading.Tasks;
using hydrogen.General.Collections;
using Microsoft.AspNetCore.Mvc;
using Nebula.Job;
using Nebula.Multitenancy;
using Nebula.Storage;
using Nebula.Storage.Model;
using Nebula.Web.Controllers.Dto;

namespace Nebula.Web.Controllers
{
    [ApiController]
    public class JobInfoController : ControllerBase
    {
        // GET	/jobs/list	-B, SAFE	Get the list of running / pending / recently finished jobs
        // GET	/jobs/j/:j	-B, SAFE    Enquiry the status / result of an asynchronous operation, using the ID returned when starting the job


        public IJobStore JobStore { get; set; }
        public IJobManager JobManager { get; set; }
        public ITenantProvider Tenant { get; set; }

        [HttpGet("jobs/list")]
        public async Task<IActionResult> GetJobList(string tenantId)
        {
            var allJobs = await JobStore.LoadAll(tenantId);
            return Ok(new GetJobListResponse
            {
                Jobs = allJobs.Select(ToGetJobListResponseItem).ToList()
            });
        }

        [HttpGet("jobs/j/{jobId}")]
        public async Task<IActionResult> GetJobStatus(string jobId)
        {
            var jobData = await JobStore.Load(Tenant.Id, jobId);
            if (jobData == null)
            {
                return NotFound();
            }

            var result = ToGetJobStatusResponse(jobData);

            // TODO Multiple-loading of job data when calling this API. Once above, and once in getting queue length
            result.QueueLength = await JobManager.GetQueueLength(Tenant.Id, jobId);

            return Ok(result);
        }

        #region helpers

        private GetJobListResponseItem ToGetJobListResponseItem(JobData jobData)
        {
            return new GetJobListResponseItem
            {
                JobId = jobData.JobId,
                State = jobData.Status.State.ToString(),
                StateTime = jobData.Status.StateTime,
                IsCompleted = jobData.Status.State >= JobState.Completed,
                JobDisplayName = jobData.JobDisplayName,
                CreationTime = jobData.CreationTime
            };
        }

        private GetJobStatusResponse ToGetJobStatusResponse(JobData jobData)
        {
            return new GetJobStatusResponse
            {
                JobId = jobData.JobId,
                JobDisplayName = jobData.JobDisplayName,
                State = jobData.Status.State.ToString(),
                StateTime = jobData.Status.StateTime,
                IsCompleted = jobData.Status.State >= JobState.Completed,
                LastActivityTime = jobData.Status.LastIterationStartTime,
                LastProcessTime = jobData.Status.LastProcessFinishTime,
                LastHealthCheckTime = jobData.Status.LastHealthCheckTime,
                ItemsProcessed = jobData.Status.ItemsProcessed,
                ItemsFailed = jobData.Status.ItemsFailed,
                ItemsRequeued = jobData.Status.ItemsRequeued,
                ItemsGeneratedForTargetQueue = jobData.Status.ItemsGeneratedForTargetQueue,
                EstimatedTotalItems = jobData.Status.EstimatedTotalItems,
                ProcessingTimeTakenMillis = jobData.Status.ProcessingTimeTakenMillis,
                ExceptionCount = jobData.Status.ExceptionCount,
                LastExceptionTime = jobData.Status.LastExceptionTime,
                LastFailTime = jobData.Status.LastFailTime,
                LastFailures = jobData.Status.LastFailures.SafeSelect(f => f.ErrorMessage).ToArray(),
                CreationTime = jobData.CreationTime,
                PreprocessorJobIds = jobData.Configuration.PreprocessorJobIds
            };
        }

        #endregion
    }
}