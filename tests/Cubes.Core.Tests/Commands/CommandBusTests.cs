using System;
using System.Collections.Generic;
using Cubes.Core.Commands;
using Microsoft.Extensions.Logging;
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
            var loggerFactoryMock = GetMockLoggerFactory();
            var factoryMock = mockRepository.Create<ServiceFactory>(MockBehavior.Strict);
            factoryMock
                .Setup(f => f(typeof(ICommandHandler<SampleCommand, SampleResult>)))
                .Returns(new SampleHandler());
            factoryMock
                .Setup(f => f(typeof(IEnumerable<ICommandBusMiddleware<SampleCommand, SampleResult>>)))
                .Returns(new ICommandBusMiddleware<SampleCommand, SampleResult>[] { });

            var bus = new CommandBus(factoryMock.Object, loggerFactoryMock);
            var command = new SampleCommand { ID = 1 };
            
            var result = bus.Submit(command);

            Assert.IsType<SampleResult>(result);
            Assert.Equal("CommandID: 1 Everything OK", result.Message);
        }

        [Fact]
        public void When_NullSubmitted_ArgumentNullExceptionRaised()
        {
            var loggerFactoryMock = GetMockLoggerFactory(setup: false);
            var bus = new CommandBus(mockRepository.Create<ServiceFactory>().Object, loggerFactoryMock);
            Assert.Throws<ArgumentNullException>(() =>
            {
                bus.Submit((SampleCommand)null);
            });
        }

        [Fact]
        public void When_UnregisteredCommandSubmitted_CommandHandlerResolveExceptionRaised()
        {
            var loggerFactoryMock = GetMockLoggerFactory();
            var factoryMock = mockRepository.Create<ServiceFactory>(MockBehavior.Strict);
            
            factoryMock
                .Setup(f => f(typeof(IEnumerable<ICommandBusMiddleware<SampleCommand, SampleResult>>)))
                .Returns(new ICommandBusMiddleware<SampleCommand, SampleResult>[] { });
            var bus = new CommandBus(factoryMock.Object, loggerFactoryMock);
            Action act = () => bus.Submit(new SampleCommand());

            var exc = Record.Exception(act);

            Assert.NotNull(exc);
            Assert.IsType<CommandHandlerResolveException>(exc);
            Assert.Equal(typeof(SampleCommand), ((CommandHandlerResolveException)exc).CommandType);
        }

        [Fact]
        public void When_MiddlewareIsRegistered_ResultCanBeMutated()
        {
            var loggerFactoryMock = GetMockLoggerFactory();
            var factoryMock = mockRepository.Create<ServiceFactory>(MockBehavior.Strict);
            factoryMock
                .Setup(f => f(typeof(ICommandHandler<SampleCommand, SampleResult>)))
                .Returns(new SampleHandler());
            factoryMock
                .Setup(f => f(typeof(IEnumerable<ICommandBusMiddleware<SampleCommand, SampleResult>>)))
                .Returns(new ICommandBusMiddleware<SampleCommand, SampleResult>[] { new SampleMiddleware() });

            var bus = new CommandBus(factoryMock.Object, loggerFactoryMock);
            var command = new SampleCommand { ID = 1 };
            var result = bus.Submit(command);

            Assert.IsType<SampleResult>(result);
            Assert.Equal("Mutated by middleware", result.Message);
        }

        public ILoggerFactory GetMockLoggerFactory(bool setup = true)
        {
            var loggerMock = mockRepository.Create<ILogger<CommandBus>>();
            var loggerFactoryMock = mockRepository.Create<ILoggerFactory>();
            loggerFactoryMock
                  .Setup(l => l.CreateLogger(It.IsAny<string>()))
                  .Returns(loggerMock.Object);
            if (setup)
            {
                loggerMock
                .Setup(i => i.Log(
                    It.IsAny<LogLevel>(),
                    0,
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()));
            }
            return loggerFactoryMock.Object;
        }
    }

    class SampleMiddleware : ICommandBusMiddleware<SampleCommand, SampleResult>
    {
        public SampleResult Execute(SampleCommand command, CommandHandlerDelegate<SampleResult> next)
        {
            var result = next();
            result.Message = "Mutated by middleware";

            return result;
        }
    }
}