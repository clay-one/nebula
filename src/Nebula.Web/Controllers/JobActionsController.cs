using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nebula.Web.Controllers
{
    [ApiController]
    public class JobActionsController : ControllerBase
    {
        // PUT	/jobs/j/:jobId/actions/resume   	-B, IDMP	Changes a job's status back to "InProgress" to continue working normally, if the job is paused or draining.
        // PUT	/jobs/j/:jobId/actions/pause    	-B, IDMP	Changes a job's status to "Paused", and stops workers of the job temporarily until resumed.
        // PUT	/jobs/j/:jobId/actions/drain    	-B, IDMP	Changes a job's status to "Draining", and causes the job queue items to be drained without processing.
        // PUT	/jobs/j/:jobId/actions/stop	        -B, IDMP	Stops a job and all its preprocessors permanently, and deletes all of the job queue items.
        // PUT	/jobs/j/:jobId/actions/purge-queue	-B, IDMP	Purges the current job queue without any change in the job status.

        [HttpPut("jobs/j/{jobId}/actions/resume")]
        public async Task<IActionResult> ResumeJob(string jobId)
        {
            return await Task.FromResult(StatusCode((int) HttpStatusCode.NotImplemented));
        }

        [HttpPut("jobs/j/{jobId}/actions/pause")]
        public async Task<IActionResult> PauseJob(string jobId)
        {
            return await Task.FromResult(StatusCode((int) HttpStatusCode.NotImplemented));
        }

        [HttpPut("jobs/j/{jobId}/actions/drain")]
        public async Task<IActionResult> DrainJob(string jobId)
        {
            return await Task.FromResult(StatusCode((int) HttpStatusCode.NotImplemented));
        }

        [HttpPut("jobs/j/{jobId}/actions/stop")]
        public async Task<IActionResult> StopJob(string jobId)
        {
            return await Task.FromResult(StatusCode((int) HttpStatusCode.NotImplemented));
        }

        [HttpPut("jobs/j/{jobId}/actions/purge-queue")]
        public async Task<IActionResult> PurgeJobQueue(string jobId)
        {
            return await Task.FromResult(StatusCode((int) HttpStatusCode.NotImplemented));
        }
    }
}