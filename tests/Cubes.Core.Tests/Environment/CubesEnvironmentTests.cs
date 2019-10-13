using Cubes.Core.Environment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using System;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Cubes.Core.Tests.Environment
{
    public class CubesEnvironmentTests : IDisposable
    {
        private MockRepository mockRepository;

        public CubesEnvironmentTests() =>
            this.mockRepository = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mockRepository.VerifyAll();

        [Fact]
        public void PrepareEnvironmentFolders_CreatesNeededFolders()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var loggerMock = mockRepository.Create<ILogger>();
            loggerMock
                .Setup(i => i.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<FormattedLogValues>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
            var unitUnderTest = new CubesEnvironment("C:\\Cubes", loggerMock.Object, fileSystem);

            // Act
            unitUnderTest.PrepareEnvironment();

            // Assert
            loggerMock.Verify(m => m.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<FormattedLogValues>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()), Times.Exactly(2));
            
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetAppsFolder()));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetStorageFolder()));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetSettingsFolder()));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.StaticContent)));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.Temp)));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.Logs)));
        }

    }
}
