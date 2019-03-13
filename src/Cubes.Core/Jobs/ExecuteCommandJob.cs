using System;
using System.Threading.Tasks;
using Cubes.Core.Commands;
using Cubes.Core.Email;
using Cubes.Core.Settings;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Quartz;

namespace Cubes.Core.Jobs
{
    public class ExecuteCommandJob : BaseQuartzJob
    {
        private readonly ILogger<ExecuteCommandJob> logger;
        private readonly ISerializer serializer;
        private readonly ITypeResolver typeResolver;
        private readonly ICommandBus commandBus;
        private readonly ISettingsProvider settingsProvider;
        private readonly IEmailDispatcher emailDispatcher;

        public ExecuteCommandJob(ILogger<ExecuteCommandJob> logger,
            ISerializer serializer,
            ITypeResolver typeResolver,
            ICommandBus commandBus,
            ISettingsProvider settingsProvider,
            IEmailDispatcher emailDispatcher)
        {
            this.logger = logger;
            this.serializer = serializer;
            this.typeResolver = typeResolver;
            this.commandBus = commandBus;
            this.settingsProvider = settingsProvider;
            this.emailDispatcher = emailDispatcher;
        }
        public override Task ExecuteInternal(IJobExecutionContext context)
        {
            JobParameter jobParameter = null;
            try
            {
                var prmAsString = context.JobDetail.JobDataMap.GetString(JobScheduler.PARAMETERS_KEY);
                jobParameter = JobParameter.FromJson(prmAsString);
            }
            catch (Exception x)
            {
                logger.LogError(x, "Could not deserialize job parameters for ExecuteCommandJob!");
                throw new JobExecutionException(x);
            }

            try
            {
                var commandType = typeResolver.GetByName(jobParameter.CommandType);
                var command     = ((JObject)jobParameter.CommandInstance).ToObject(commandType);

                logger.LogInformation($"Executing '{commandType.Name}' ...");
                var result = commandBus.Submit(jobParameter.CommandInstance);
                if (result.ExecutionResult != CommandExecutionResult.Success)
                {
                    logger.LogWarning(result.Message);
                    SetResult(result.Message);
                }
                else
                {
                    // If results contain an EmailContent property sent email
                    var email = result.GetPropertyByType<EmailContent>();
                    if (email != null)
                    {
                        // Results can contain an instance of SmtpSettings to use
                        var settings = result.GetPropertyByType<SmtpSettings>() ??
                            settingsProvider.Load<SmtpSettings>();
                        settings.ThrowIfNull("SmtpSettings");
                        emailDispatcher.DispatchEmail(email, settings);
                    }
                    SetResult(result.Message);
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