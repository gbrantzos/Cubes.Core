using System;
using System.Collections.Generic;

namespace Cubes.Core.DataAccess
{
    /// <summary>
    /// Query executor to support Q from CQRS.
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Execute query and convert results to TResult 
        /// </summary>
        /// <typeparam name="TResult">Return object type</typeparam>
        /// <param name="namedConnection">Name of connection</param>
        /// <param name="sqlQuery"><see cref="SqlQuery"/> to execute</param>
        /// <param name="parameterValues">Paramter values</param>
        /// <param name="columnToProperty">Column to parameters mappings</param>
        /// <param name="afterPopulating">Action to execute after object is populated</param>
        /// <returns></returns>
        IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> afterPopulating = null) where TResult : class, new();

        /// <summary>
        /// Execute query and convert results to dynamic  
        /// </summary>
        /// <param name="namedConnection">Name of connection</param>
        /// <param name="sqlQuery"><see cref="SqlQuery"/> to execute</param>
        /// <param name="parameterValues">Paramter values</param>
        /// <param name="columnToProperty">Column to parameters mappings</param>
        /// <returns></returns>
        IEnumerable<dynamic> Query(
            string namedConnection,
            SqlQuery sqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null);

        /// <summary>
        /// Execute named query and convert results to TResult
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="namedConnection">Name of connection</param>
        /// <param name="namedSqlQuery"></param>
        /// <param name="parameterValues">Paramter values</param>
        /// <param name="columnToProperty">Column to parameters mappings</param>
        /// <param name="afterPopulating">Action to execute after object is populated</param>
        /// <returns></returns>
        IEnumerable<TResult> Query<TResult>(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null,
            Action<TResult> afterPopulating = null) where TResult : class, new();

        /// <summary>
        /// Execute named query and convert results to dynamic
        /// </summary>
        /// <param name="namedConnection">Name of connection</param>
        /// <param name="namedSqlQuery"><see cref="SqlQuery"/> to execute</param>
        /// <param name="parameterValues">Paramter values</param>
        /// <param name="columnToProperty">Column to parameters mappings</param>
        /// <returns></returns>
        IEnumerable<dynamic> Query(
            string namedConnection,
            string namedSqlQuery,
            Dictionary<string, object> parameterValues,
            Dictionary<string, string> columnToProperty = null);
    }
}
