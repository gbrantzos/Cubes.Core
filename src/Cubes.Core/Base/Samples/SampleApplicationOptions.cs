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
        public string ConnectionString { get; set; }
        public string Endpoint { get; set; }
        public bool CheckExistence { get; set; }
        public List<string> CheckExistenceExceptions { get; set; }
        public List<User> Users { get; set; }
        public SampleApplicationOptions()
        {
            ConnectionString = "Connection string ...";
            CheckExistenceExceptions = Enumerable.Empty<string>().ToList();

            Users = new List<User>();
        }
    }

    public class User
    {
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
