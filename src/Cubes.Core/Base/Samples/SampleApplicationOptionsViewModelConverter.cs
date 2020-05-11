using System;
using System.Linq;
using Cubes.Core.Web.UIHelpers;
using Cubes.Core.Web.UIHelpers.Schema;

namespace Cubes.Core.Base.Samples
{
    public class SampleApplicationOptionsViewModelConverter : ViewModelConverter
    {
        public override object FromViewModel(object viewModel)
        {
            var toReturn = new SampleApplicationOptions();
            dynamic temp = viewModel;

            toReturn.ConnectionString = temp.Basic.ConnectionString;
            toReturn.Endpoint = temp.Basic.Endpoint;
            toReturn.CheckExistence = temp.Basic.CheckExistence;

            string tempValue = temp.Basic.CheckExistenceExceptions;
            toReturn.CheckExistenceExceptions = tempValue
                .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.StartsWith("#"))
                .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(i => i.Trim())
                .Where(i => !String.IsNullOrEmpty(i))
                .ToList();

            foreach (var user in temp.WmsUsers)
            {
                toReturn.Users.Add(new User
                {
                    DisplayName = user.DisplayName,
                    UserName    = user.UserName,
                    Password    = user.Password
                });
            }

            return toReturn;
        }

        public override object ToViewModel(object configurationInstance)
        {
            if (!(configurationInstance is SampleApplicationOptions config))
                throw new ArgumentException($"Could not cast to {nameof(SampleApplicationOptions)}");

            return new
            {
                Basic = new
                {
                    config.ConnectionString,
                    config.Endpoint,
                    config.CheckExistence,
                    CheckExistenceExceptions = String.Join(", ", config.CheckExistenceExceptions.ToArray())
                },
                config.Users
            };
        }
    }
}
