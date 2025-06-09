using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.LocalImps
{
    public class UploadServiceLocal : IUploadService
    {
        private FtpClient ftpClient;

        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;

        public FtpClient FtpClient
        {
            get { return ftpClient; } 
            set { ftpClient = value; }
        }
        public UploadServiceLocal(FtpClient ftpClient) 
        {
            this.ftpClient = ftpClient;
        }

        public async Task<ResultResponse<bool>> UploadFile(string filePath, string fileName, bool hasProgress = false)
        {
            if (hasProgress)
            {
                ftpClient.InvokeProgressChanged += InvokeProgressChanged;
            }
            var result = await ftpClient.UploadFileToFtpServer(filePath, fileName);

            if (hasProgress)
            {
                ftpClient.InvokeProgressChanged -= InvokeProgressChanged;
            }

            return new ResultResponse<bool>
            {
                Code = 200,
                Data = result,
                Message = "Successful"
            };
        }

        public async Task<ResultResponse<bool>> DownloadFile(string fileName)
        {
            var result = await ftpClient.DownloadFile(fileName);

            return new ResultResponse<bool>
            {
                Code = 200,
                Data = result,
                Message = "Successful"
            };
        }
    }
}
