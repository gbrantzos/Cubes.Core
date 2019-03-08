using System;

namespace Cubes.Api.Controllers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerCategoryAttribute : Attribute
    {
        public SwaggerCategoryAttribute(string prefix)
            => Prefix = prefix;

        public string Prefix { get; }
    }
}