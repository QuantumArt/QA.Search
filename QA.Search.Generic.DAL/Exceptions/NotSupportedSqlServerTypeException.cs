using QA.Search.Generic.DAL.Services.Configuration;
using System;
using System.Runtime.Serialization;

namespace QA.Search.Generic.DAL.Exceptions
{
    [Serializable]
    public class NotSupportedSqlServerTypeException : Exception
    {
        public NotSupportedSqlServerTypeException(SqlServerType sqlServerType) : base($"Current sql server type - \"{sqlServerType}\"") { }
        public NotSupportedSqlServerTypeException(string message) : base(message) { }
        public NotSupportedSqlServerTypeException(string message, Exception inner) : base(message, inner) { }
        protected NotSupportedSqlServerTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
