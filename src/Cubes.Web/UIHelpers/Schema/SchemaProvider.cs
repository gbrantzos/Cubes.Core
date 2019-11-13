namespace Cubes.Web.UIHelpers.Schema
{
    public abstract class SchemaProvider<T> : ISchemaProvider
    {
        public virtual string Name => typeof(T).Name;
        public abstract Schema GetSchema();
    }
}