﻿using System.Collections.Generic;

namespace QA.Search.Admin.Models.ElasticManagementPage
{
    public class ElasticManagementPageResponse
    {
        public bool Loading { get; set; }
        public bool CommonError { get; set; }

        public List<IndexesCardViewModel> Cards { get; set; }
    }
}
