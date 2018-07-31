using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using hydrogen.General.Validation;

namespace Nebula.Controllers
{
    public abstract class ApiControllerBase : ApiController
    {
        protected IHttpActionResult NotImplemented()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        protected IHttpActionResult ValidationResult(ApiValidationResult result)
        {
            if (result.Success)
                return Ok();

            return new ValidationErrorResult(result, this);
        }

        protected IHttpActionResult ValidatedResult<T>(ApiValidatedResult<T> result)
        {
            if (result.Success)
                return Ok(result.Result);

            return new ValidationErrorResult(result, this);
        }

        protected IHttpActionResult ValidationError(ApiValidationError error)
        {
            return new ValidationErrorResult(ApiValidationResult.Failure(error), this);
        }

        protected IHttpActionResult ValidationError(IEnumerable<ApiValidationError> errors)
        {
            return new ValidationErrorResult(ApiValidationResult.Failure(errors), this);
        }

        protected IHttpActionResult ValidationError(string errorKey, IEnumerable<string> errorParams = null)
        {
            return new ValidationErrorResult(errorParams == null
                ? ApiValidationResult.Failure(errorKey)
                : ApiValidationResult.Failure(errorKey, errorParams), this);
        }

        protected IHttpActionResult ValidationError(string propertyPath, string errorKey)
        {
            return new ValidationErrorResult(ApiValidationResult.Failure(propertyPath, errorKey), this);
        }
    }
}