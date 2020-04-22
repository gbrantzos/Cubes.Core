using System;
using System.IO.Abstractions.TestingHelpers;
using Cubes.Core.Base;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cubes.Core.Tests.Environment
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
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
                    It.IsAny<Object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Object, Exception, string>>()))
                .Verifiable();
            var unitUnderTest = new CubesEnvironment("C:\\Cubes", "", new ApplicationManifest[] { }, loggerMock.Object, fileSystem);

            // Act
            unitUnderTest.PrepareHost();

            // Assert
            loggerMock.Verify(m => m.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<Object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Object, Exception, string>>()), Times.Exactly(2));

            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetStorageFolder()));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetSettingsFolder()));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.Content)));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.Temp)));
            Assert.True(fileSystem.Directory.Exists(unitUnderTest.GetFolder(CubesFolderKind.Logs)));
        }

    }
}
