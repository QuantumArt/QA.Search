using NJsonSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Api.Infrastructure
{
    /// <summary>
    /// Кеш JSON-схем для валидации запросов
    /// </summary>
    public static class JsonSchemaRegistry
    {
        private static readonly SemaphoreSlim AsyncLock = new SemaphoreSlim(1);

        private static readonly Dictionary<string, JsonSchema> SchemaCache = new Dictionary<string, JsonSchema>();

        /// <summary>
        /// Получить JSON-схему по пути к её .json файлу
        /// </summary>
        public static async ValueTask<JsonSchema> GetSchema(string schemaPath)
        {
            schemaPath = Path.Combine(AppContext.BaseDirectory, schemaPath);

            try
            {
                if (!SchemaCache.TryGetValue(schemaPath, out JsonSchema schema))
                {
                    await AsyncLock.WaitAsync();

                    if (!SchemaCache.TryGetValue(schemaPath, out schema))
                    {
                        string schemaJson = await File.ReadAllTextAsync(schemaPath);

                        schema = await JsonSchema.FromJsonAsync(schemaJson);

                        schema.SchemaVersion = null;
                        schema.ExtensionData?.Remove("$id");

                        SchemaCache.Add(schemaPath, schema);
                    }
                }
                return schema;
            }
            finally
            {
                AsyncLock.Release();
            }
        }
    }
}
