using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Utilities;
using Quartz;

namespace Cubes.Web.UIHelpers.Lookups
{
    public class JobTypeLookupProvider : ILookupProvider
    {
        private static readonly string ProviderName = "JobTypes";

        public string Name => ProviderName;

        public Lookup Get()
        {
            var typesList = GetJobTypes()
            .Select(i =>
            {
                var attribute = i.GetAttribute<DisplayAttribute>();
                var display = attribute == null || String.IsNullOrEmpty(attribute.Name) ? i.Name : attribute.Name;

                return new LookupItem
                {
                    Value     = i.FullName,
                    Display   = display,
                    Group     = String.Empty,
                    OtherData = null
                };
            })
            .ToList();

            return new Lookup
            {
                Name  = this.Name,
                Items = typesList
            };
        }


        private static IEnumerable<Type> GetJobTypes()
        {
            var types = new List<Type>();
            var dlls = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var dll in dlls)
                types.AddRange(dll.GetTypes()
                    .Where(i => i.IsClass && i.IsConcrete() && i.GetInterfaces().Contains(typeof(IJob)))
                    .ToList()
                );

            return types;
        }
    }
}
