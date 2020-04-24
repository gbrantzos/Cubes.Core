namespace Cubes.Core.DataAccess
{
    public interface IQueryProvider
    {
        string QueryName { get; }
        Query GetQuery();
    }
}
