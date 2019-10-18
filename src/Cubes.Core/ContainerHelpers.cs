using Autofac;
using Cubes.Core.Commands.Basic;
using Cubes.Core.Environment;
using MediatR;

namespace Cubes.Core
{
    public static class ContainerHelpers
    {
        public static ContainerBuilder RegisterCubeServices(this ContainerBuilder builder)
        {
            // Register our command bus, MediatR
            builder.RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            // Register basic jobs
            builder.RegisterType<RunOsProcessHandler>().AsImplementedInterfaces();
            builder.RegisterType<SqlResultsAsEmailHandler>().AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterApplicationServices(this ContainerBuilder builder, ICubesEnvironment cubes)
        {
            foreach (var application in cubes.GetActivatedApplications())
                application.RegisterServices(builder);

            return builder;
        }
    }
}
