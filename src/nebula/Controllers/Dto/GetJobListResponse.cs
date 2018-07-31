using System.Collections.Generic;

namespace Nebula.Controllers.Dto
{
    public class GetJobListResponse
    {
        public List<GetJobListResponseItem> Jobs { get; set; }
    }
}