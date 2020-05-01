using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Configuration;
using Cubes.Core.Web.UIHelpers;

namespace Cubes.Core.Base.Samples
{
    [ConfigurationStore(SampleApplication.SettingFile)]
    [ViewModelConverter(typeof(SampleApplicationSettingsViewModelConverter))]
    public class SampleApplicationSettings
    {
        public string SEnConnection { get; set; }
        public string OdwConnection { get; set; }
        public bool CheckEofExistence { get; set; }
        public List<string> CheckEofExistenceExceptions { get; set; }

        public SampleApplicationSettings()
        {
            SEnConnection = "Pharmex.SEn";
            OdwConnection = "Pharmex.ODW";
            CheckEofExistenceExceptions = Enumerable.Empty<string>().ToList();
        }
    }
}
