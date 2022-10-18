using System;
using System.Runtime.Serialization;

namespace QA.Search.Generic.Integration.QP.Exceptions
{
    [Serializable]
    public class PageUrlCannotMakeException : Exception
    {
        public PageUrlCannotMakeException(string relativePath) : base($"Requested relative path was: \"{relativePath}\"") { }
        public PageUrlCannotMakeException(string message, Exception inner) : base(message, inner) { }
        protected PageUrlCannotMakeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
