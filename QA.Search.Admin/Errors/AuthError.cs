using System;

namespace QA.Search.Admin.Errors
{
    public class AuthError : Exception
    {
        public AuthError(string mesage) : base(mesage) { }
    }
}
