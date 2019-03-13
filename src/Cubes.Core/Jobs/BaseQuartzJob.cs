using System;
using System.Threading.Tasks;
using Quartz;

namespace Cubes.Core.Jobs
{
    public abstract class BaseQuartzJob: IJob
    {
        public const string MESSAGE_KEY = "ResultMessage";
        public const string HASERRORS_KEY = "ResultHasErrors";

        private string resultMessage;
        private bool resultHasErrors;

        public virtual Task Execute(IJobExecutionContext context)
        {
            try
            {
                var result = ExecuteInternal(context);

                if (!String.IsNullOrEmpty(resultMessage))
                    context.JobDetail.JobDataMap.Add(MESSAGE_KEY, resultMessage);
                context.JobDetail.JobDataMap.Add(HASERRORS_KEY, false.ToString());

                return result;
            }
            catch (Exception x)
            {
                context.JobDetail.JobDataMap.Add(HASERRORS_KEY, true.ToString());
                context.JobDetail.JobDataMap.Add(MESSAGE_KEY, x.Message);
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