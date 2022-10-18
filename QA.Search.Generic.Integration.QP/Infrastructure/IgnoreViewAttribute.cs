using System;

namespace QA.Search.Generic.Integration.QP.Infrastructure
{
    /// <summary>
    /// Исключить отмеченную <see cref="ElasticView{}"/> из процесса индексации
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IgnoreViewAttribute : Attribute
    {
    }
}
