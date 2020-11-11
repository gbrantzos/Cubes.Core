using System;
using System.Linq;

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

        public override string ToString() => $"{Value} {Policy}";

        /// <summary>
        /// Create a retention instance from a string, example '10 Days' or '12 Executions'.
        /// </summary>
        /// <param name="retentionString"></param>
        /// <returns></returns>
        public static Retention FromString(string retentionString)
        {
            if (retentionString.Equals("LastWeek", StringComparison.OrdinalIgnoreCase))
                return Retention.LastWeek;
            if (retentionString.Equals("LastTen", StringComparison.OrdinalIgnoreCase))
                return Retention.LastTen;

            var tmp = retentionString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tmp.Length != 2)
                throw new ArgumentException($"Invalid retention string format: {retentionString}");
            if (!Int32.TryParse(tmp[0], out int value))
                throw new ArgumentException($"Invalid retention value: {tmp[0]}");
            if (!new string[] { "DAYS", "EXECUTIONS" }.Contains(tmp[1].ToUpper()))
                throw new ArgumentException($"Invalid retention policy: {tmp[1]}");

            return new Retention { Policy = Enum.Parse<RetentionPolicy>(tmp[1], true), Value = value };
        }
    }
}