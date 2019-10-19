using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cubes.Core.Environment
{
    // TODO Needs documentation
    public abstract class Application : IApplication
    {
        public virtual IConfigurationBuilder ConfigureAppConfiguration(IConfigurationBuilder configuration) => configuration;

        public virtual IServiceCollection ConfigureServices(IServiceCollection services) => services;

        public virtual ContainerBuilder RegisterServices(ContainerBuilder builder) => builder;

        public virtual string SwaggerXmlFile => String.Empty;
    }
}
