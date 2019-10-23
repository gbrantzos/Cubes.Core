using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Cubes.Web
{
    public static class Extensions
    {
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
    }
}
