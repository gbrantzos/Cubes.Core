using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cubes.Core.Utilities;
using Xunit;

namespace Cubes.Core.Tests.Utilities
{
    public class CompressedFileProviderTests
    {
        [Fact]
        public void Scratch()
        {
            var zipFile = @"C:\Users\gbrantzos\Downloads\RoslynPad.zip";
            var fs = new CompressedFileProvider(zipFile);
            var fi = fs.GetFileInfo("Test/AvalonLibrary.dll");

            Assert.NotNull(fi);
            Assert.Equal(307712, fi.Length);

            var all = fs.GetDirectoryContents("");
            var ww = new StreamWriter($@"C:\Users\gbrantzos\Downloads\{fi.Name}");
            fi.CreateReadStream().CopyTo(ww.BaseStream);
            ww.Flush();
        }
    }
}
