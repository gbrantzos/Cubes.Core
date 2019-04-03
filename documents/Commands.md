# Commands
Cubes uses a command bus implementation as a method of encapsulating business logic units. The main goal of this architecture is to provide a clean, simple way of expressing what should be done and how this can be accomplished.

The main parts of this architecture are the following
- Command object
- Result object
- Command Bus
- Handler


## Command
A command object is a simple POCO describing the intent of a use case. Commands contain all necessary information needed by the application, to perform a task.

Command implement the `ICommand<TResult>` interface, where `TResult` is the result of the corresponding command handler:
```
public class SimpleCommand : ICommand<SimpleResult>
{
    /// <summary>
    /// Command definition member
    /// </summary>
    public string Command { get; set; }
}
```
This interface has no specific definition and it mostly acts as a marker interface.
<br/><br/>

## Result
Result describes the output of the handler. It must implement the `ICommandResult` interface:
```
public class SampleResult : ICommandResult
{
    public CommandExecutionResult ExecutionResult{ get; set; }
    public string Message { get; set; }
}
```
Although you can directly implement the `ICommandResult` interface, it is suggested to inherit your results from the abstract class `BaseCommandResult`:
```
public class SampleResult : BaseCommandResult
{
    // Simple command specific output
    public string Output { get; set; }
}
```
<br/><br/>

## Command Bus
The role of the Command Bus is to match commands with handlers, and ensures that a valid result is returned to its clients after handler is executed. When a command is dispatched, the bus locates the appropriate handler and calls the handle method.

You can inject the command bus in your code by adding `ICommandBus commandBus` in the constructor of your class, for example in a controller:
```
public class CommandController : ControllerBase
{
    private readonly ICommandBus commandBus;

    public CommandController(ICommandBus commandBus)
    {
        this.commandBus = commandBus;
        ...
    }

    ...
}
```
<br/><br/>

## Handler
Handlers implement use cases. They interpret the intent of a specific Command and perform the expected behavior. They have a 1:1 relationship with Commands – meaning that for each Command, there is only ever one Handler.

Handlers implement the following the `ICommandHandler` interface:
```
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : ICommandResult
{
        TResult Handle(TCommand command);
}
```
where TCommand is the command object and TResult the result of the handler.

As with results, you can directly implement the `ICommandHandler` interface,but it is suggested to inherit your handlers from the abstract class `BaseCommandHandler<TCommand, TResult>` and override the `HandleInternal` method:
```
// Base command handler
public abstract class BaseCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : ICommandResult
{
    ...

    protected abstract TResult HandleInternal(TCommand command);
}

// Simple handler
public class SampleHandler : BaseCommandHandler<SampleCommand, SampleResult>
{
    protected override SampleResult HandleInternal(SampleCommand command)
    {
        return new SampleResult
        {
            Message = $"CommandID: {command.ID} Everything OK"
        };
    }
}
```
<br/><br/>

