namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public class Retention
    {
        public enum RetentionPolicy
        {
            Days,
            Executions
        }

        public RetentionPolicy Policy { get; set; }
        public int Value { get; set; }

        public static Retention LastWeek = new Retention { Policy = RetentionPolicy.Days, Value = 7 };
        public static Retention LastTen  = new Retention { Policy = RetentionPolicy.Executions, Value = 10 };
    }
}