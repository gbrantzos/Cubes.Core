namespace Cubes.Core.DataAccess
{
    public interface IQueryManager
    {
        Query GetSqlQuery(string queryName);
    }
}