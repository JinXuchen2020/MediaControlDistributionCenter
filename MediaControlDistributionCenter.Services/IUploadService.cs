using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.DTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services
{
    public interface IUploadService
    {
        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;

        public Task<ResultResponse<bool>> UploadFile(string filePath, string fileName, bool hasProgress = false);

        public Task<ResultResponse<bool>> DownloadFile(string fileName);
    }
}
