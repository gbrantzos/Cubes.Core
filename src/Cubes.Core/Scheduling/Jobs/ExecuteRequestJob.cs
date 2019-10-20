using System;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.Email;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;

namespace Cubes.Core.Scheduling.Jobs
{
    // TODO Revisit
    public class ExecuteRequestJob : BaseQuartzJob
    {
        class JobParameterInternal
        {
            public string CommandType { get; set; }
            public string CommandInstance { get; set; }
        }

        private readonly ILogger<ExecuteRequestJob> logger;
        private readonly ITypeResolver typeResolver;
        private readonly IMediator mediator;
        private readonly IEmailDispatcher emailDispatcher;
        private readonly SmtpSettingsProfiles smtpProfiles;

        public ExecuteRequestJob(ILogger<ExecuteRequestJob> logger,
            ITypeResolver typeResolver,
            IMediator mediator,
            IOptionsSnapshot<SmtpSettingsProfiles> options,
            IEmailDispatcher emailDispatcher)
        {
            this.logger           = logger;
            this.typeResolver     = typeResolver;
            this.mediator         = mediator;
            this.emailDispatcher  = emailDispatcher;
            this.smtpProfiles     = options.Value;
        }

        public override Task ExecuteInternal(IJobExecutionContext context)
        {
            JobParameterInternal jobParameter = null;
            try
            {
                var prmAsString = context.JobDetail.JobDataMap.GetString("MUST BE A CONSTANT"); // TODO Check this!
                var jObject = JObject.Parse(prmAsString);
                jobParameter = new JobParameterInternal
                {
                    CommandType     = jObject.GetValue("CommandType").ToString(),
                    CommandInstance = jObject.GetValue("CommandInstance").ToString()
                };
            }
            catch (Exception x)
            {
                logger.LogError(x, "Could not deserialize job parameters for ExecuteCommandJob!");
                throw new JobExecutionException(x);
            }

            try
            {
                var type    = typeResolver.GetByName(jobParameter.CommandType);
                var command = JsonConvert.DeserializeObject(jobParameter.CommandInstance, type);

                logger.LogInformation($"Executing '{command.GetType().Name}' ...");
                var result = mediator.Send(command);
                //if (result.ExecutionResult != CommandExecutionResult.Success)
                //{
                //    logger.LogWarning(result.Message);
                //}
                //else
                {
                    // If results contain an EmailContent property sent email
                    var email = result.GetPropertyByType<EmailContent>();
                    if (email != null)
                    {
                        // Results can contain an instance of SmtpSettings to use
                        var settings = result.GetPropertyByType<SmtpSettings>() ??
                            smtpProfiles.Profiles.First();
                        settings.ThrowIfNull("SmtpSettings");
                        emailDispatcher.DispatchEmail(email, settings);
                    }
                }
            }
            catch (Exception x)
            {
                logger.LogError(x, "Could not execute command!");
                throw new JobExecutionException(x);
            }
            return Task.CompletedTask;
        }
    }
}
