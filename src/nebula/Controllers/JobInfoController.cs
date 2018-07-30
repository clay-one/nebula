using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ComposerCore.Attributes;
using hydrogen.General.Collections;
using Nebula.Controllers.Dto;
using Nebula.Job;
using Nebula.Storage;
using Nebula.Storage.Model;

namespace Nebula.Controllers
{
    [Contract]
    [Component]
    [ComponentCache(null)]
    public class JobInfoController : ApiControllerBase
    {
        // GET	/jobs/list	-B, SAFE	Get the list of running / pending / recently finished jobs
        // GET	/jobs/j/:j	-B, SAFE Enquiry the status / result of an asynchronous operation, using the ID returned when starting the job

        [ComponentPlug]
        public IJobStore JobStore { get; set; }

        [ComponentPlug]
        public IJobManager JobManager { get; set; }

        [HttpGet]
        [Route("jobs/list")]
        public async Task<IHttpActionResult> GetJobList(string tenantId)
        {
            var allJobs = await JobStore.LoadAll(tenantId);
            return Ok(new GetJobListResponse
            {
                Jobs = allJobs.Select(ToGetJobListResponseItem).ToList()
            });
        }

        [HttpGet]
        [Route("jobs/j/{jobId}")]
        public async Task<IHttpActionResult> GetJobStatus(string tenantId, string jobId)
        {
            var jobData = await JobStore.Load(tenantId, jobId);
            if (jobData == null)
                return NotFound();

            var result = ToGetJobStatusResponse(jobData);

            // TODO Multiple-loading of job data when calling this API. Once above, and once in getting queue length
            result.QueueLength = await JobManager.GetQueueLength(tenantId, jobId);

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