using System;

namespace Cubes.Core.Web.UIHelpers
{
    public abstract class ViewModelConverter
    {
        public abstract object ToViewModel(object configurationInstance);
        public abstract object FromViewModel(object viewModel);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewModelConverterAttribute : Attribute
    {
        public Type ViewModelConverterType { get; set; }

        public ViewModelConverterAttribute(Type type)
        {
            if (!type.IsSubclassOf(typeof(ViewModelConverter)))
                throw new ArgumentException($"Type {type.Name} does not derive from ViewModelConverter!");

            ViewModelConverterType = type;
        }
    }
}
