using MediaControlDistributionCenter.Helpers.FTP.Server;
using OpenCvSharp.Internal;
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
            ftpServer.FtpServerStart();
        }

        public async Task<bool> UploadFileToFtpServer(string filePath, string fileName = "")
        {
            var loginCmd = $"USER {ftpServer._userName}\n";
            var verifyCmd = $"PASS {ftpServer._userPwd}\n";
            fileName = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
            var storeCmd = $"STOR {fileName}\n";
            var result = false;
            using (TcpClient tcpClient = new TcpClient(ftpServer._Ip, int.Parse(ftpServer._port)))
            {
                var stream = tcpClient.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                string connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!connectRsp.Contains("220"))
                {
                    return false;
                }

                byte[] loginData = Encoding.UTF8.GetBytes(loginCmd);
                stream.Write(loginData, 0, loginData.Length);

                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!connectRsp.Contains("331"))
                {
                    return false;
                }

                byte[] verifyData = Encoding.UTF8.GetBytes(verifyCmd);
                stream.Write(verifyData, 0, verifyData.Length);

                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!connectRsp.Contains("230"))
                {
                    return false;
                }

                byte[] fileData = File.ReadAllBytes(filePath);

                byte[] fileSizeData = Encoding.UTF8.GetBytes($"FILESIZE {fileData.LongLength}\n");
                stream.Write(fileSizeData, 0, fileSizeData.Length);

                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!connectRsp.Contains("225"))
                {
                    return false;
                }

                byte[] storeData = Encoding.UTF8.GetBytes(storeCmd);
                stream.Write(storeData, 0, storeData.Length);

                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);

                connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!connectRsp.Contains("150"))
                {
                    return false;
                }

                stream.Write(fileData, 0, fileData.Length);
                byte[] endSignal = Encoding.ASCII.GetBytes("\n");
                stream.Write(endSignal, 0, endSignal.Length);

                buffer = new byte[1024];
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                connectRsp = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (connectRsp.Contains("226"))
                {
                    return true;
                }
            }

            return result;
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
