namespace Cubes.Core.Commands
{
    /// <summary>
    /// Command bus
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Submit a Command for execution 
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <returns></returns>
        TResult Submit<TResult>(ICommand<TResult> command);
    }
}