namespace Cubes.Core.Commands
{
    /// <summary>
    /// Handler of Command with given type
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <typeparam name="TResult">Result type</typeparam>
    public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
    {
         TResult Handle(TCommand command);
    }
}