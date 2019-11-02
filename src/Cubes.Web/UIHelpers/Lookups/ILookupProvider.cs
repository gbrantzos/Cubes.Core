namespace Cubes.Web.UIHelpers.Lookups
{
    public interface ILookupProvider
    {
        string Name { get; }
        Lookup Get();
    }
}
