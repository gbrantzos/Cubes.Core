using MediatR;

namespace Cubes.Core.Commands
{
    /// <summary>
    /// Base request class to be used as ancestor for all <see cref="IRequest"/> objects
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class Request<TResult> : IRequest<Result<TResult>> { }
}
