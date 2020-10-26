using System;

namespace Cubes.Core.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnSettingsAttribute : Attribute
    {
        public string Header { get; set; }

        public ColumnSettingsAttribute(string header)
        {
            Header = header;
        }
    }
}
