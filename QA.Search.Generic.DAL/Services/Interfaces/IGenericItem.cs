using QA.Search.Generic.DAL.Models;
using System;

namespace QA.Search.Generic.DAL.Services.Interfaces
{
    public interface IGenericItem
    {
        int ContentItemID { get; set; }
        StatusType StatusType { get; set; }
        bool Visible { get; set; }
        bool Archive { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
    }
}
