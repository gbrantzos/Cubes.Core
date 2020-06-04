using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Base
{
    /// <summary>
    /// Application class, with basic implementation
    /// </summary>
    public abstract class Application : IApplication
    {
        public virtual IConfigurationBuilder ConfigureAppConfiguration(
            IConfigurationBuilder configuration,
            ICubesEnvironment cubes) => configuration;

        public virtual IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
            => services;

        public virtual ContainerBuilder RegisterServices(ContainerBuilder builder)
            => builder;

        public virtual IEnumerable<string> GetSwaggerXmlFiles()
            => Enumerable.Empty<string>();

        public virtual IEnumerable<ApplicationOptionsUIConfig> GetUIConfig()
            => Enumerable.Empty<ApplicationOptionsUIConfig>();

        protected string ApplicationPath()
            => Path.GetDirectoryName(this.GetType().Assembly.Location);

        protected string GetFullPathForFile(string fileName)
            => Path.Combine(this.ApplicationPath(), fileName);

        public virtual IEnumerable<(string, Func<object>)> ConfigurationInitializers()
            => Enumerable.Empty<(string, Func<object>)>();
    }
}
