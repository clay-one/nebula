using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Results;
using hydrogen.General.Validation;

namespace Nebula.Controllers
{
    public class ValidationErrorResult : NegotiatedContentResult<ApiValidationResult>
    {
        public ValidationErrorResult(ApiValidationResult content, IContentNegotiator contentNegotiator,
            HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(HttpStatusCode.BadRequest, content, contentNegotiator, request, formatters)
        {
        }

        public ValidationErrorResult(ApiValidationResult content, ApiController controller)
            : base(HttpStatusCode.BadRequest, content, controller)
        {
        }
    }
}