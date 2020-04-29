namespace Cubes.Core.Web.UIHelpers.Lookups
{
    public interface ILookupProvider
    {
        /// <summary>
        /// Name of lookup
        /// </summary>
        string Name { get; }

        /// <summary>
        /// <see cref="Lookup"/> produced by provider.
        /// </summary>
        /// <returns></returns>
        Lookup GetLookup();
    }
}
