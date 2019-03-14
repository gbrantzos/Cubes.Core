using System;
using System.Threading.Tasks;
using Quartz;

namespace Cubes.Core.Jobs
{
    public abstract class BaseQuartzJob: IJob
    {


        private string resultMessage;
        private bool resultHasErrors;

        public virtual Task Execute(IJobExecutionContext context)
        {
            try
            {
                var result = ExecuteInternal(context);

                if (!String.IsNullOrEmpty(resultMessage))
                    context.JobDetail.JobDataMap.Add(QuartzJobDataParameters.MESSAGE_KEY, resultMessage);

                return result;
            }
            catch (Exception x)
            {
                context.JobDetail.JobDataMap.Add(QuartzJobDataParameters.MESSAGE_KEY, x.Message);
                throw new JobExecutionException(x);
            }
        }

        public abstract Task ExecuteInternal(IJobExecutionContext context);

        protected void SetResult(string message, bool hasErrors)
        {
            resultMessage = message;
            resultHasErrors = hasErrors;
        }

        protected void SetResult(string message) => SetResult(message, false);
    }
}