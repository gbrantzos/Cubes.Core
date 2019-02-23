using System;
using Cubes.Core.Commands;
using Moq;
using Xunit;

namespace Cubes.Core.Tests.Commands
{
    public class CommandBusTest : IDisposable
    {
        private MockRepository mockRepository;
        public CommandBusTest() => mockRepository = new MockRepository(MockBehavior.Strict);
        public void Dispose() => mockRepository.VerifyAll();

        [Fact]
        public void When_SampleCommandSubmitted_SampleResultReturned()
        {
            var factoryMock = mockRepository.Create<CommandHandlerFactory>(MockBehavior.Strict);
            factoryMock
                .Setup(f => f(typeof(ICommandHandler<SampleCommand, SampleResult>)))
                .Returns(new SampleHandler());

            var bus = new CommandBus(factoryMock.Object);
            var command = new SampleCommand { ID = 1 };
            
            var result = bus.Submit(command);

            Assert.IsType<SampleResult>(result);
            Assert.Equal("CommandID: 1 Everything OK", result.Message);
        }

        [Fact]
        public void When_NullSubmitted_ArgumentNullExceptionRaised()
        {
            var bus = new CommandBus(mockRepository.Create<CommandHandlerFactory>().Object);
            Assert.Throws<ArgumentNullException>(() =>
            {
                bus.Submit((SampleCommand)null);
            });
        }

        [Fact]
        public void When_UnregisteredCommandSubmitted_CommandHandlerResolveExceptionRaised()
        {
            var bus = new CommandBus(mockRepository.Create<CommandHandlerFactory>().Object);
            Action act = () => bus.Submit(new SampleCommand());

            var exc = Record.Exception(act);

            Assert.NotNull(exc);
            Assert.IsType<CommandHandlerResolveException>(exc);
            Assert.Equal(typeof(SampleCommand), ((CommandHandlerResolveException)exc).CommandType);
        }
    }
}