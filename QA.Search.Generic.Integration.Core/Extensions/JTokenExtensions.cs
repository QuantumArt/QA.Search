using Newtonsoft.Json.Linq;
using System;

namespace QA.Search.Generic.Integration.Core.Extensions
{
    public static class JTokenExtensions
    {
        /// <summary>
        /// Visit all objects in JSON tree
        /// </summary>
        public static void VisitObjects(this JToken token, Action<JObject> action)
        {
            if (token is JObject obj)
            {
                action.Invoke(obj);

                foreach (JToken value in obj.PropertyValues())
                {
                    value.VisitObjects(action);
                }
            }
            else if (token is JArray arr)
            {
                foreach (JToken element in arr)
                {
                    element.VisitObjects(action);
                }
            }
        }
    }
}