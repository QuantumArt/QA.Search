using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Linq;

namespace QA.Search.Api.Infrastructure
{
    public static class JsonSchemaExtensions
    {
        /// <summary>
        /// Провалидировать объект <paramref name="model"/> по схеме <paramref name="schema"/>
        /// и пердставить ошибки в формате <see cref="ValidationProblemDetails"/>
        /// </summary>
        public static bool TryValidateModel(
            this JsonSchema schema, JToken model, out ValidationProblemDetails problemDetails)
        {
            var vaidationErrors = schema.Validate(model);

            if (vaidationErrors.Count == 0)
            {
                problemDetails = null;
                return true;
            }

            var validatinoErrors = vaidationErrors
                .GroupBy(error => error.Path)
                .ToDictionary(
                    group => group.Key.Substring(2).Replace('/', '.'),
                    group => group.Select(error => error.Kind.ToString()).ToArray()
                );

            problemDetails = new ValidationProblemDetails(validatinoErrors);

            return false;
        }
    }
}