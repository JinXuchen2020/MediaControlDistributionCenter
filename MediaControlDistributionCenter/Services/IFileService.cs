using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IFileService
    {
        public void CreateResourcePath(string resPath);

        public string SaveFileContent(string fileDicPath, string fileName, string fileContent);

        public void DeleteFileContent(string filePath);

        public string SaveFileContent(string fileDicPath, string fileName, Stream fileContent);

        public T? ReadFileContent<T>(string fileDicPath, string fileName, params JsonConverter[] converters);

        public void CreatZip(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel = CompressionLevel.NoCompression, bool includeBaseDirectory = true);
    }
}
