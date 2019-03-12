namespace Cubes.Core.Jobs
{
    public interface IJobScheduler
    {
         void LoadJobs();
         SchedulerStatus GetStatus();
         SchedulerStatus StartScheduler();
         SchedulerStatus StopScheduler();
    }
}