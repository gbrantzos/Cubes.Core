using System;
using System.Threading.Tasks;
using Quartz;

namespace Cubes.Core.Scheduling.Jobs
{
    public abstract class BaseQuartzJob: IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            try
            {
                var result = ExecuteInternal(context);
                return result;
            }
            catch (Exception x)
            {
                throw new JobExecutionException(x);
            }
        }

        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}