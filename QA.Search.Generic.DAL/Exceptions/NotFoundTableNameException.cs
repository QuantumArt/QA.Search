using System;
using System.Runtime.Serialization;

namespace QA.Search.Generic.DAL.Exceptions
{
    [Serializable]
    public class NotFoundTableNameException : Exception
    {
        public NotFoundTableNameException(string findTable) : base($"Desired table - \"{findTable}\"") { }
        public NotFoundTableNameException(string message, Exception inner) : base(message, inner) { }
        protected NotFoundTableNameException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
