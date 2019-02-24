namespace Cubes.Core.Commands
{
    public delegate TResult CommandHandlerDelegate<TResult>();

    public interface ICommandBusMiddleware<TCommand, TResult> 
    {
        TResult Execute(TCommand command, CommandHandlerDelegate<TResult> next);
    }
}
