namespace Cubes.Core.Commands
{
    public delegate TResult CommandHandlerDelegate<TResult>() where TResult : ICommandResult;

    public interface ICommandBusMiddleware<TCommand, TResult> where TResult : ICommandResult
    {
        TResult Execute(TCommand command, CommandHandlerDelegate<TResult> next);
    }
}
