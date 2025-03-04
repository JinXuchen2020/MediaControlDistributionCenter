using System.IO;
using System.IO.IsolatedStorage;

using Microsoft.EntityFrameworkCore;

namespace MediaControlDistributionCenter.Helpers
{
    internal class StorageHelper
    {
        public static void SaveToFile(string text,string fileName= "mcdcp.dat")
        {
            //string encryptedPassword = EncryptionHelper.GetSha256Hash(text);

            using var storage = IsolatedStorageFile.GetUserStoreForApplication();
            using var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, storage);
            using var writer = new StreamWriter(stream);
            writer.Write(text);
        }

        public static string LoadFromFile(string fileName= "mcdcp.dat")
        {
            if(!IsolatedStorageFile.GetUserStoreForApplication().FileExists(fileName))
            {
                return string.Empty;
            }
            using var storage = IsolatedStorageFile.GetUserStoreForApplication();
            using var stream = new IsolatedStorageFileStream(fileName, FileMode.Open, storage);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        public static void DeleteFile(string fileName = "mcdcp.dat")
        {
            if (IsolatedStorageFile.GetUserStoreForApplication().FileExists(fileName))
            {
                IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(fileName);
            }
        }
    }
}
