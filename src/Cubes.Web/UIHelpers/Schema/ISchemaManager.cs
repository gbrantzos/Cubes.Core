namespace Cubes.Web.UIHelpers.Schema
{
    public interface ISchemaManager
    {
        /// <summary>
        /// Get <see cref="Schema"/> identified by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of schema</param>
        /// <returns></returns>
        Schema GetSchema(string name);
    }
}