using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cubes.Core.Web
{
    public static class Extensions
    {
        /// <summary>
        /// Convert <see cref="IQueryCollection"/> (query string) to <see cref="String"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string AsString(this IQueryCollection query)
        {
            var sb = new StringBuilder();
            if (query.Count !=0)
            {
                char seperator = '?';

                foreach (var item in query)
                {
                    sb.Append(seperator);
                    sb.Append(item.Key).Append('=').Append(item.Value);
                    seperator = '&';
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert any object to JSON
        /// </summary>
        /// <param name="object">Object to convert</param>
        /// <param name="serializerSettings">Json serializer settings</param>
        /// <returns></returns>
        public static string AsJson(this object @object, JsonSerializerSettings serializerSettings = null)
        {
            var settings = serializerSettings ?? new JsonSerializerSettings();
            return JsonConvert.SerializeObject(@object, settings);
        }

        /// <summary>
        /// Check if <see cref="HttpResponse"/> is successful.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool IsSuccess(this HttpResponse response)
            => response.StatusCode >= 200 && response.StatusCode <= 299 && response.StatusCode != 204;
    }
}
