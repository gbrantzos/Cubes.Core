namespace Cubes.Web.UIHelpers.Schema
{
    /// <summary>
    /// Base class for implementing an <see cref="ISchemaProvider"/>.
    /// </summary>
    public abstract class SchemaProvider<T> : ISchemaProvider
    {
        /// <summary>
        /// The name used to register <see cref="Schema"/>. Defaults to full type name of <typeparamref name="T"/>.
        /// </summary>
        public virtual string Name => typeof(T).FullName;

        /// <summary>
        /// The <see cref="Schema"/> created by provider.
        /// </summary>
        /// <returns></returns>
        public abstract Schema GetSchema();
    }
}