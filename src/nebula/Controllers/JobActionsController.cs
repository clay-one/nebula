using System.Threading.Tasks;
using System.Web.Http;
using ComposerCore.Attributes;
using hydrogen.General.Validation;
using Nebula.Job;
using Nebula.Multitenancy;

namespace Nebula.Controllers
{
    [Contract]
    [Component]
    [ComponentCache(null)]
    public class JobActionsController : ApiControllerBase
    {
        // PUT	/jobs/j/:j/actions/resume	-B, IDMP	Changes a job's status back to "InProgress" to continue working normally, if the job is paused or draining.
        // PUT	/jobs/j/:j/actions/pause	-B, IDMP	Changes a job's status to "Paused", and stops workers of the job temporarily until resumed.
        // PUT	/jobs/j/:j/actions/drain	-B, IDMP	Changes a job's status to "Draining", and causes the job queue items to be drained without processing.
        // PUT	/jobs/j/:j/actions/stop	-B, IDMP	Stops a job and all its preprocessors permanently, and deletes all of the job queue items.
        // PUT	/jobs/j/:j/actions/purge-queue	-B, IDMP	Purges the current job queue without any change in the job status.

        [ComponentPlug]
        public IJobManager JobManager { get; set; }

        [ComponentPlug]
        public ITenantProvider Tenant { get; set; }

        [HttpPut]
        [Route("jobs/j/{jobId}/actions/resume")]
        public async Task<IHttpActionResult> ResumeJob(string jobId)
        {
            var validationResult = ValidateForResumeJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.ResumeJob(Tenant.Id, jobId));
        }

        [HttpPut]
        [Route("jobs/j/{jobId}/actions/pause")]
        public async Task<IHttpActionResult> PauseJob(string jobId)
        {
            var validationResult = ValidateForPauseJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.PauseJob(Tenant.Id, jobId));
        }

        [HttpPut]
        [Route("jobs/j/{jobId}/actions/drain")]
        public async Task<IHttpActionResult> DrainJob(string jobId)
        {
            var validationResult = ValidateForDrainJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.DrainJob(Tenant.Id, jobId));
        }

        [HttpPut]
        [Route("jobs/j/{jobId}/actions/stop")]
        public async Task<IHttpActionResult> StopJob(string jobId)
        {
            var validationResult = ValidateForStopJob(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.StopJob(Tenant.Id, jobId));
        }

        [HttpPut]
        [Route("jobs/j/{jobId}/actions/purge-queue")]
        public async Task<IHttpActionResult> PurgeJobQueue(string jobId)
        {
            var validationResult = ValidateForPurgeJobQueue(jobId);
            if (!validationResult.Success)
                return ValidationResult(validationResult);

            return ValidationResult(await JobManager.PurgeJobQueue(Tenant.Id, jobId));
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