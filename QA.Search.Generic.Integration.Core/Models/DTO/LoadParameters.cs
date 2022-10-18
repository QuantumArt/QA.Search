using System;

namespace QA.Search.Generic.Integration.Core.Models.DTO
{
    public class LoadParameters
    {
        public int FromID { get; set; }
        public DateTime FromDate { get; set; }
        public ViewParameters ViewParameters { get; set; }
    }
}
