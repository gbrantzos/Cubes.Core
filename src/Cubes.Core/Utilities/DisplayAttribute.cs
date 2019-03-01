using System;

namespace Cubes.Core.Utilities
{
    public class DisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Hide { get; set; }

        public DisplayAttribute(string name, int order, bool hide)
        {
            Name  = name;
            Order = order;
            Hide  = hide;
        }
        public DisplayAttribute(string name) : this(name, 0, false) { }
        public DisplayAttribute(string name, int order) : this(name, order, false) { }
    }
}