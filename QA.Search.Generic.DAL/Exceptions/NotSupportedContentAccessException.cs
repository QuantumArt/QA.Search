using QA.Search.Generic.DAL.Services.Configuration;
using System;
using System.Runtime.Serialization;

namespace QA.Search.Generic.DAL.Exceptions
{
    [Serializable]
    public class NotSupportedContentAccessException : Exception
    {
        public NotSupportedContentAccessException(ContentAccess contentAccess) : base($"Current content access - \"{contentAccess}\"") { }
        public NotSupportedContentAccessException(string message) : base(message) { }
        public NotSupportedContentAccessException(string message, Exception inner) : base(message, inner) { }
        protected NotSupportedContentAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
