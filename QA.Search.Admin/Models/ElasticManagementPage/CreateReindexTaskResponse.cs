using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Models.ElasticManagementPage
{
    public class CreateReindexTaskResponse
    {
        public bool TaskCreated { get; set; }
        public string ErrorMessage { get; set; }
    }
}
