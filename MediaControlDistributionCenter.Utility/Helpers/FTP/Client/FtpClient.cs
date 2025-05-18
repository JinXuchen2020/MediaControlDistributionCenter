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

        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;

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
                request.KeepAlive = false;
                request.Credentials = new NetworkCredential(ftpServer._userName, ftpServer._userPwd);
                byte[] fileContents = File.ReadAllBytes(filePath);

                request.ContentLength = fileContents.Length;

                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    int bufferSize = fileContents.Length / 100;
                    int bytesSent = 0;
                    int totalBytes = fileContents.Length;

                    using (MemoryStream memoryStream = new MemoryStream(fileContents))
                    {
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead;

                        while ((bytesRead = await memoryStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                        {
                            await requestStream.WriteAsync(buffer, 0, bytesRead);
                            bytesSent += bytesRead;

                            // 计算并显示进度
                            double progressPercentage = (bytesSent * 100.0) / totalBytes;
                            InvokeProgressChanged?.Invoke(this, new ProgressEventArgs(progressPercentage));
                            if (progressPercentage == 100)
                            {
                                break;
                            }
                        }
                    }
                    //requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync()))
                {
                    Log.Information(response.StatusCode.ToString());
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
            using (var response = (FtpWebResponse)(await request.GetResponseAsync()))
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, fileName);
                    using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await stream.CopyToAsync(fileStream);
                    return true;
                }
            }
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }

        public ProgressEventArgs(double progress)
        {
            this.Progress = progress;
        }
    }
}
