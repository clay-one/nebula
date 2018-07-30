using System.Threading.Tasks;
using System.Web.Http;
using ComposerCore.Attributes;
using hydrogen.General.Validation;
using Nebula.Job;

namespace Nebula.Controllers
{
    [Contract]
    [Component]
    [ComponentCache(null)]
    public class JobActionsController : ApiControllerBase
    {
        // PUT	/tenant/:tenantId/jobs/:j/actions/resume	-B, IDMP	Changes a job's status back to "InProgress" to continue working normally, if the job is paused or draining.
        // PUT	/tenant/:tenantId/jobs/:j/actions/pause	-B, IDMP	Changes a job's status to "Paused", and stops workers of the job temporarily until resumed.
        // PUT	/tenant/:tenantId/jobs/:j/actions/drain	-B, IDMP	Changes a job's status to "Draining", and causes the job queue items to be drained without processing.
        // PUT	/tenant/:tenantId/jobs/:j/actions/stop	-B, IDMP	Stops a job and all its preprocessors permanently, and deletes all of the job queue items.
        // PUT	/tenant/:tenantId/jobs/:j/actions/purge-queue	-B, IDMP	Purges the current job queue without any change in the job status.

        [ComponentPlug]
        public IJobManager JobManager { get; set; }

        [HttpPut]
        [Route("tenant/{tenantId}/jobs/{jobId}/actions/resume")]
        public async Task<IHttpActionResult> ResumeJob(string tenantId, string jobId)
        {
            var validationResult = ValidateForResumeJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.ResumeJob(tenantId, jobId));
        }

        [HttpPut]
        [Route("tenant/{tenantId}/jobs/{jobId}/actions/pause")]
        public async Task<IHttpActionResult> PauseJob(string tenantId, string jobId)
        {
            var validationResult = ValidateForPauseJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.PauseJob(tenantId, jobId));
        }

        [HttpPut]
        [Route("tenant/{tenantId}/jobs/{jobId}/actions/drain")]
        public async Task<IHttpActionResult> DrainJob(string tenantId, string jobId)
        {
            var validationResult = ValidateForDrainJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.DrainJob(tenantId, jobId));
        }

        [HttpPut]
        [Route("tenant/{tenantId}/jobs/{jobId}/actions/stop")]
        public async Task<IHttpActionResult> StopJob(string tenantId, string jobId)
        {
            var validationResult = ValidateForStopJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.StopJob(tenantId, jobId));
        }

        [HttpPut]
        [Route("tenant/{tenantId}/jobs/{jobId}/actions/purge-queue")]
        public async Task<IHttpActionResult> PurgeJobQueue(string tenantId, string jobId)
        {
            var validationResult = ValidateForPurgeJobQueue(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.PurgeJobQueue(tenantId, jobId));
        }

        #region Validation methods

        private ApiValidationResult ValidateForResumeJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return ApiValidationResult.Failure(nameof(jobId), ErrorKeys.ArgumentCanNotBeEmpty);

            return ApiValidationResult.Ok();
        }

        private ApiValidationResult ValidateForPauseJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return ApiValidationResult.Failure(nameof(jobId), ErrorKeys.ArgumentCanNotBeEmpty);

            return ApiValidationResult.Ok();
        }

        private ApiValidationResult ValidateForDrainJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return ApiValidationResult.Failure(nameof(jobId), ErrorKeys.ArgumentCanNotBeEmpty);

            return ApiValidationResult.Ok();
        }

        private ApiValidationResult ValidateForStopJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return ApiValidationResult.Failure(nameof(jobId), ErrorKeys.ArgumentCanNotBeEmpty);

            return ApiValidationResult.Ok();
        }

        private ApiValidationResult ValidateForPurgeJobQueue(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return ApiValidationResult.Failure(nameof(jobId), ErrorKeys.ArgumentCanNotBeEmpty);

            return ApiValidationResult.Ok();
        }

        #endregion
    }
}