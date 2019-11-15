namespace Cubes.Core.DataAccess
{
    public interface IQueryManager
    {
        /// <summary>
        /// Get <see cref="Query"/> stored in configuration
        /// </summary>
        /// <param name="queryName">Name of query</param>
        /// <returns></returns>
        Query GetSqlQuery(string queryName);
    }
}