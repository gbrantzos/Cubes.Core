namespace Cubes.Core.Commands
{
    public class RequestLoggingOptions
    {
        /// <summary>
        /// Array with type names for whom failures should be logged. Names can include wildcards (*).
        /// </summary>
        public string[] LogFailedRequests { get; set; } = new string[] { };

        /// <summary>
        /// Array of type names for whom exceptions should be logged. Names can include wildcards (*).
        /// </summary>
        public string[] LogExceptionsForRequests { get; set; } = new string[] { };
    }
}
