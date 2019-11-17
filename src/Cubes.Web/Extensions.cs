using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Cubes.Web
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
        /// Check if <see cref="HttpResponse"/> is successful.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool IsSuccess(this HttpResponse response)
            => response.StatusCode >= 200 && response.StatusCode <= 299 && response.StatusCode != 204;
    }
}
