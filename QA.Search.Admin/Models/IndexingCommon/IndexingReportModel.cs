using QA.Search.Generic.Integration.Core.Models;
using System;

namespace QA.Search.Admin.Models.IndexingCommon
{
    public class IndexingReportModel
    {
        public IndexingReportModel(IndexingReport report)
        {
            DocumentsLoadTime = report.DocumentsLoadTime;
            DocumentsProcessTime = report.DocumentsProcessTime;
            DocumentsIndexTime = report.DocumentsIndexTime;
            IdsLoaded = report.IdsLoaded;
            ProductsLoaded = report.ProductsLoaded;
            ProductsIndexed = report.ProductsIndexed;
            ProductsNotIndexed = report.ProductsNotIndexed;
            BatchSize = report.BatchSize;
            IndexName = report.IndexName;
        }


        public TimeSpan DocumentsLoadTime { get; set; }
        public TimeSpan DocumentsProcessTime { get; set; }
        public TimeSpan DocumentsIndexTime { get; set; }

        public int IdsLoaded { get; set; }
        public int ProductsLoaded { get; set; }
        public int ProductsIndexed { get; set; }
        public int ProductsNotIndexed { get; set; }

        public int BatchSize { get; set; }
        public string IndexName { get; set; }
    }
}
