using Cubes.Core.Commands;

namespace Cubes.Core.Tests.Commands
{
    public class SampleHandler : ICommandHandler<SampleCommand, SampleResult>
    {
        public SampleResult Handle(SampleCommand command)
        {
            return new SampleResult
            {
                Message = $"CommandID: {command.ID} Everything OK"
            };
        }
    }
}