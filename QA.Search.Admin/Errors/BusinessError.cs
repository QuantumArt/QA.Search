using System;
namespace QA.Search.Admin.Errors
{
    public class BusinessError : Exception
    {
        //TODO log all business error
        public BusinessError(string message) : base(message) { }
    }
}
