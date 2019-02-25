using Cubes.Core.Utilities;
using System;

namespace Cubes.Core.Settings
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SettingsPrefixAttribute : Attribute
    {
        public string Prefix { get; set; }

        public SettingsPrefixAttribute(string prefix) => this.Prefix = prefix.ThrowIfEmpty(nameof(prefix));
    }
}