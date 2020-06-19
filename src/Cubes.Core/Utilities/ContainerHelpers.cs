using System.Reflection;
using Autofac;
using Cubes.Core.Security;
using Cubes.Core.Base;
using Cubes.Core.Commands;
using Cubes.Core.Commands.Basic;
using MediatR;
using MediatR.Pipeline;

namespace Cubes.Core.Utilities
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

            // Simple MediatR pipeline
            builder.RegisterGeneric(typeof(DefaultBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            // We could also add
            // - History of execution
            // - Auditing and security

            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).AsImplementedInterfaces();

            // Register basic jobs
            builder.RegisterType<RunOsProcessHandler>().AsImplementedInterfaces();
            builder.RegisterType<RunOsProcessValidator>().AsImplementedInterfaces();

            builder.RegisterType<ArchiveFilesHandler>().AsImplementedInterfaces();
            builder.RegisterType<QueryResultsAsEmailHandler>().AsImplementedInterfaces();

            builder.RegisterType<AuthenticateUserHandler>().AsImplementedInterfaces();
            builder.RegisterType<AuthenticateUserValidator>().AsImplementedInterfaces();
            builder.RegisterType<SaveUserHandler>().AsImplementedInterfaces();
            builder.RegisterType<SaveUserValidator>().AsImplementedInterfaces();
            builder.RegisterType<SaveRolesHandler>().AsImplementedInterfaces();
            builder.RegisterType<SaveRolesValidator>().AsImplementedInterfaces();
            builder.RegisterType<ResetUserPasswordHandler>().AsImplementedInterfaces();
            builder.RegisterType<ResetUserPasswordValidator>().AsImplementedInterfaces();

            builder.RegisterType<GetUsersHandler>().AsImplementedInterfaces();
            builder.RegisterType<GetRolesHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteUserHandler>().AsImplementedInterfaces();

            // Register serializers, use them by name
            builder.RegisterType<JsonSerializer>()
                .As<ISerializer>()
                .Keyed<ISerializer>(CubesConstants.Serializer_JSON);
            builder.RegisterType<YamlSerializer>()
                .As<ISerializer>()
                .Keyed<ISerializer>(CubesConstants.Serializer_YAML);

            return builder;
        }

        public static ContainerBuilder RegisterApplicationServices(this ContainerBuilder builder, ICubesEnvironment cubes)
        {
            foreach (var application in cubes.GetApplicationInstances())
                application.RegisterServices(builder);

            return builder;
        }

        public static ContainerBuilder RegisterCubesRequestHandlers(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (assemblies == null)
                assemblies = new Assembly[] { Assembly.GetCallingAssembly() };
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsCubesRequestHandler())
                .AsImplementedInterfaces();
            return builder;
        }

        public static ContainerBuilder RegisterCubesRequestValidators(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (assemblies == null)
                assemblies = new Assembly[] { Assembly.GetCallingAssembly() };
            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.IsValidator())
                .AsImplementedInterfaces();
            return builder;
        }
    }
}
