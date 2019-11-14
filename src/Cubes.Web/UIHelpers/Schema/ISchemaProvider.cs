namespace Cubes.Web.UIHelpers.Schema
{
    public interface ISchemaProvider
    {
        /// <summary>
        /// Name of produced schema
        /// </summary>
        string Name { get; }

        /// <summary>
        /// <see cref="Schema"/> produced by provider.
        /// </summary>
        /// <returns></returns>
        Schema GetSchema();
    }
}
