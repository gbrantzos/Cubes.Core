using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Cubes.Core.Commands;
using Cubes.Core.Email;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Cubes.Core.Scheduling.Jobs
{
    public class ExecuteRequestJob : BaseQuartzJob
    {
        private static readonly string Request_Type          = "RequestType";
        private static readonly string Request_Instance      = "RequestInstance";
        //private static readonly string Dispatcher_Type       = "DispatcherType";
        //private static readonly string Dispatcher_Parameters = "DispatcherParameters";

        private readonly ILogger<ExecuteRequestJob> logger;
        private readonly IMediator mediator;
        private readonly ISerializer serializer;
        private readonly ITypeResolver typeResolver;
        private readonly IEmailDispatcher emailDispatcher;
        private readonly SmtpSettingsProfiles smtpProfiles;

        public ExecuteRequestJob(ILogger<ExecuteRequestJob> logger,
            IMediator mediator,
            [KeyFilter(CubesConstants.Serializer_YAML)] ISerializer serializer,
            ITypeResolver typeResolver,
            IEmailDispatcher emailDispatcher,
            IOptionsSnapshot<SmtpSettingsProfiles> options)
        {
            this.logger           = logger;
            this.mediator         = mediator;
            this.serializer       = serializer;
            this.typeResolver     = typeResolver;
            this.emailDispatcher  = emailDispatcher;
            this.smtpProfiles     = options.Value;
        }

        public override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var jobParams = context
                .JobDetail
                .JobDataMap
                .Where(d => d.Value is string)
                .ToDictionary(kv => kv.Key, kv => kv.Value as string);

            if (!jobParams.TryGetValue(Request_Type, out var requestTypeName))
                throw new ArgumentException("No request type defined!");

            var requestType = typeResolver.GetByName(requestTypeName);
            if (requestType == null)
                throw new ArgumentException($"Could not resolve type {requestTypeName}!");

            if (!jobParams.TryGetValue(Request_Instance, out var requestInstanceRaw))
                throw new ArgumentException("No request instance defined!");

            var requestInstance = serializer.Deserialize(requestInstanceRaw, requestType);
            if (requestInstance == null)
                throw new ArgumentException("Could not create request instance!");

            logger.LogInformation($"Executing '{requestType.Name}', {requestInstance} ...");
            var requestResponse = await mediator.Send(requestInstance);

            if (requestResponse is IResult)
            {
                var result = requestResponse as IResult;
                if (result.HasErrors)
                {
                    if (result.ExceptionThrown == null)
                        logger.LogWarning(result.Message);
                    else
                        logger.LogError(result.ExceptionThrown, result.ExceptionThrown.Message);
                }
                else
                {
                    logger.LogInformation(result.Message);
                    DispatchResult(result.Response/*, jobParams*/);
                }
            }
        }

        private void DispatchResult(object response/*, Dictionary<string, string> jobParams*/)
        {
            // This part will be responsible for creating and using the appropriate dispatcher
            /*
            if (!jobParams.TryGetValue(Dispatcher_Type, out var dispatcherTypeName))
                return;

            var dispatcherType = typeResolver.GetByName(dispatcherTypeName);
            if (dispatcherType == null)
                throw new ArgumentException($"Could not resolve dispatcher with type {dispatcherTypeName}!");
                */

            var dispatcher = emailDispatcher;
            {
                // If results contain an EmailContent property sent email
                var email = response.GetPropertyByType<EmailContent>();
                if (email != null)
                {
                    // Results can contain an instance of SmtpSettings to use
                    var settings = response.GetPropertyByType<SmtpSettings>() ?? smtpProfiles.Profiles.FirstOrDefault();
                    settings.ThrowIfNull("SmtpSettings");
                    dispatcher.DispatchEmail(email, settings);
                }
            }
        }
    }
}
