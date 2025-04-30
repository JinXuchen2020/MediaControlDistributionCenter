using MediaControlDistributionCenter.Helpers.FTP.Server;
using OpenCvSharp.Internal;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Helpers.FTP.Client
{
    public class FtpClient
    {
        private readonly FtpServer ftpServer;

        public FtpClient(FtpServer ftpServer) 
        {
            this.ftpServer = ftpServer;
            if (!ftpServer.IsStarted)
            {
                ftpServer.FtpServerStart();
            }
        }

        public async Task<bool> UploadFileToFtpServer(string filePath, string fileName = "")
        {
            try
            {
                fileName = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpServer._Ip}:{ftpServer._port}/{fileName}");
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UsePassive = false;
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(ftpServer._userName, ftpServer._userPwd);
                byte[] fileContents = File.ReadAllBytes(filePath);

                request.ContentLength = fileContents.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.ClosingData;
                }
            }
            catch (Exception ex) 
            {
                Log.Error(ex.Message);
                return false;
            }
        }

        public async Task<bool> DownloadFile(string fileName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{ftpServer._Ip}:{ftpServer._port}/{fileName}");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UsePassive = false;
            request.UseBinary = true;
            request.Credentials = new NetworkCredential(ftpServer._userName, ftpServer._userPwd);
            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, fileName);
                    using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    stream.CopyTo(fileStream);
                    return true;
                }
            }
        }
    }
}
