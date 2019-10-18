using Cubes.Core.Commands;

namespace Cubes.Core.Tests.Commands
{
    public class SampleCommand : IRequest<SampleResult>
    {
        public int ID { get; set; }
        public string Description { get; set; }
        
    }
}