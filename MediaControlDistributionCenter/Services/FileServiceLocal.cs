using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public class FileServiceLocal :IFileService
    {
        public FileServiceLocal() { }

        public void CreateResourcePath(string resPath)
        {
            if(!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
        }

        public string SaveFileContent(string fileDicPath, string fileName, string fileContent)
        {
            this.CreateResourcePath(fileDicPath);
            var configFilePath = Path.Combine(fileDicPath, fileName);
            File.WriteAllText(configFilePath, fileContent);
            return configFilePath;
        }

        public string SaveFileContent(string fileDicPath, string fileName, Stream fileContent)
        {
            this.CreateResourcePath(fileDicPath);
            var configFilePath = Path.Combine(fileDicPath, fileName);
            using (FileStream stream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write))
            {
                fileContent.CopyTo(stream);
            }

            return configFilePath;
        }

        public T? ReadFileContent<T>(string fileDicPath, string fileName, params JsonConverter[] converters)
        {
            var configFilePath = Path.Combine(fileDicPath, fileName);
            if (!File.Exists(configFilePath))
            {
                return default(T);
            }
            var fileContent = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<T>(fileContent, converters);
        }

        public void CreatZip(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel = CompressionLevel.NoCompression, bool includeBaseDirectory = true)
        {
            if (Directory.Exists(sourceDirectoryName))
            {
                if (File.Exists(destinationArchiveFileName))
                {
                    File.Delete(destinationArchiveFileName);
                }

                ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory);
            }
        }

        public void DeleteFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
