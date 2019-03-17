using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using Cubes.Core.Settings;
using KellermanSoftware.CompareNetObjects;
using Moq;
using Xunit;

namespace Cubes.Core.Tests.Settings
{
    public class JsonFileSettingsProviderTests : IDisposable
    {
        private MockRepository mockRepository;

        public JsonFileSettingsProviderTests()
            => this.mockRepository = new MockRepository(MockBehavior.Strict);

        public void Dispose() => mockRepository.VerifyAll();

        [Fact]
        public void When_NoFileExists_ProviderCreatesDefaultValue()
        {
            var fsMock = new MockFileSystem();
            var rootPath = "C:/Cubes/Settings";
            fsMock.AddDirectory(rootPath);
            var provider = new JsonFilesSettingsProvider(rootPath, fsMock);
            var settings = provider.Load<SampleSettings>();

            // Get correct type name
            var expected = "SampleSettings.json";
            var actual   = (string)GetPrivateMethod<JsonFilesSettingsProvider>("CreateFileName")
                .Invoke(provider, new object[] { settings.GetType(), String.Empty });
            var settingsFullPath = $"{rootPath}/{expected}";

            Assert.Equal(expected, actual);
            Assert.True(fsMock.FileExists(settingsFullPath));
        }

        [Fact]
        public void When_SettingsAttributeAndKeyAreUsed_CorrectNameCreated()
        {
            var fsMock = new MockFileSystem();
            var rootPath = "C:/Cubes/Settings";
            fsMock.AddDirectory(rootPath);
            var provider = new JsonFilesSettingsProvider(rootPath, fsMock);

            var expected = "Sample.SampleSettingsWithAttribute.Key1.json";
            var actual   = (string)GetPrivateMethod<JsonFilesSettingsProvider>("CreateFileName")
                .Invoke(provider, new object[] { typeof(SampleSettingsWithAttribute), "Key1" });
            var settingsFullPath = $"{rootPath}/{expected}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void When_SettingsSaved_CorrectFileCreated()
        {
            var fsMock = new MockFileSystem();
            var rootPath = "C:/Cubes/Settings";
            fsMock.AddDirectory(rootPath);
            var provider = new JsonFilesSettingsProvider(rootPath, fsMock);

            var settings = new SampleSettings
            {
                ID = 32,
                Description = "This is a sample description"
            };
            provider.Save(settings);

            var settingsFullPath = $"{rootPath}/SampleSettings.json";
            var actual = fsMock.GetFile(settingsFullPath).TextContents;
            var expected = String.Format("{{{0}  \"ID\": 32,{0}  \"Description\": \"This is a sample description\"{0}}}", System.Environment.NewLine);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void When_SettingsLoaded_CorrectObjectInstantiated()
        {
            var fsMock = new MockFileSystem();
            var rootPath = "C:/Cubes/Settings";

            var expected = new SampleSettings
            {
                ID = 32,
                Description = "This is a sample description"
            };
            var fileData = String.Format("{{{0}  \"ID\": 32,{0}  \"Description\": \"This is a sample description\"{0}}}", System.Environment.NewLine);
            fsMock.AddDirectory(rootPath);
            fsMock.AddFile($"{rootPath}/SampleSettings.json", new MockFileData(fileData));

            var provider = new JsonFilesSettingsProvider(rootPath, fsMock);
            var actual = provider.Load<SampleSettings>();

            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(expected, actual);

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void When_KeyEqualsDefault_KeyIsSetToEmpty()
        {
            var fsMock = new MockFileSystem();
            var rootPath = "C:/Cubes/Settings";
            fsMock.AddDirectory(rootPath);
            var provider = new JsonFilesSettingsProvider(rootPath, fsMock);

            var expected = "Sample.SampleSettingsWithAttribute.json";
            var actual = (string)GetPrivateMethod<JsonFilesSettingsProvider>("CreateFileName")
                .Invoke(provider, new object[] { typeof(SampleSettingsWithAttribute), "default" });
            var settingsFullPath = $"{rootPath}/{expected}";

            Assert.Equal(expected, actual);
        }

        private MethodInfo GetPrivateMethod<T>(string methodName)
        {
            var type = typeof(T);
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).FirstOrDefault(i => i.Name == "CreateFileName");
        }
    }

    public class SampleSettings
    {
        public int ID { get; set; }
        public String Description { get; set; }
    }

    [SettingsPrefix("Sample")]
    public class SampleSettingsWithAttribute
    {
        public int ID { get; set; }
        public String Description { get; set; }
    }


}