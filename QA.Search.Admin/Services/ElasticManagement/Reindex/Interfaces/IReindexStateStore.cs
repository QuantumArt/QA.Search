using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    public interface IReindexStateStore
    {
        IReindexState GetState();
    }
}
