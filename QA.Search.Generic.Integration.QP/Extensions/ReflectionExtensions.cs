using System;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    internal static class ReflectionExtensions
    {
        public static bool IsNumericType(this object o)
        {
            if (o != null)
            {
                switch (Type.GetTypeCode(o.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return true;
                }
            }
            return false;
        }
    }
}
