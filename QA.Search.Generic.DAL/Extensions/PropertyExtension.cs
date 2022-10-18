using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace QA.Search.Generic.DAL.Extensions
{
    public static class PropertyExtension
    {
        /// <summary>
        /// Получает имя столбца для этого свойства.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetPropertyColumnName(this IProperty property)
        {
            StoreObjectIdentifier? identifier = StoreObjectIdentifier.Create(property.DeclaringEntityType, StoreObjectType.Table);

            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            return property.GetColumnName((StoreObjectIdentifier)identifier);
        }
    }
}
