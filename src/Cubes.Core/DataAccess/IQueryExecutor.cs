using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Core.DataAccess
{
    public interface IQueryExecutor
    {
        IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, string> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> onCreated = null) where TResult : class, new();

        IEnumerable<dynamic> Query(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, string> parameterValues,
            Dictionary<string, string> columnToProperty = null);

        IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, string> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> onCreated = null) where TResult : class, new();

        IEnumerable<dynamic> Query(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, string> parameterValues,
            Dictionary<string, string> columnToProperty = null);

    }
}
