using System;
using System.Linq;
using Cubes.Core.Web.UIHelpers;
using Cubes.Core.Web.UIHelpers.Schema;

namespace Cubes.Core.Base.Samples
{
    public class SampleApplicationSettingsViewModelConverter : ViewModelConverter
    {
        public override object FromViewModel(object viewModel)
        {
            var toReturn = new SampleApplicationSettings();
            dynamic temp = viewModel;

            toReturn.SEnConnection = temp.Basic.SEnConnection;
            toReturn.OdwConnection = temp.Basic.OdwConnection;
            toReturn.CheckEofExistence = temp.Basic.CheckEofExistence;

            string tempValue = temp.Basic.CheckEofExistenceExceptions;
            toReturn.CheckEofExistenceExceptions = tempValue
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.StartsWith("#"))
                .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(i => i.Trim())
                .Where(i => !String.IsNullOrEmpty(i))
                .ToList();

            return toReturn;
        }

        public override object ToViewModel(object configurationInstance)
        {
            var config = configurationInstance as SampleApplicationSettings;
            if (config is null)
                throw new ArgumentException($"Could not cast to {nameof(SampleApplicationSettings)}");

            return new
            {
                Basic = new
                {
                    config.SEnConnection,
                    config.OdwConnection,
                    config.CheckEofExistence,
                    CheckEofExistenceExceptions = String.Join(", ", config.CheckEofExistenceExceptions.ToArray())
                }
            };
        }
    }
}
