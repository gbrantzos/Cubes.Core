using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Configuration;
using Cubes.Core.Web.UIHelpers;

namespace Cubes.Core.Base.Samples
{
    [ConfigurationStore(SampleApplication.OptionsFile)]
    [ViewModelConverter(typeof(SampleApplicationOptionsViewModelConverter))]
    public class SampleApplicationOptions
    {
        public string SEnConnection { get; set; }
        public string OdwConnection { get; set; }
        public bool CheckEofExistence { get; set; }
        public List<string> CheckEofExistenceExceptions { get; set; }
        public List<WmsUser> WmsUsers { get; set; }

        public SampleApplicationOptions()
        {
            SEnConnection = "Pharmex.SEn";
            OdwConnection = "Pharmex.ODW";
            CheckEofExistenceExceptions = Enumerable.Empty<string>().ToList();

            WmsUsers = new List<WmsUser>();
        }
    }

    public class WmsUser
    {
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
