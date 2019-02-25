namespace Cubes.Core.DataAccess
{
    public interface ISqlQueryManager
    {
        SqlQuery GetSqlQuery(string queryName);
    }
}