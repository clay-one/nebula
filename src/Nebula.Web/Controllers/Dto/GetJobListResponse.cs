using System.Collections.Generic;

namespace Nebula.Web.Controllers.Dto
{
    public class GetJobListResponse
    {
        public List<GetJobListResponseItem> Jobs { get; set; }
    }
}