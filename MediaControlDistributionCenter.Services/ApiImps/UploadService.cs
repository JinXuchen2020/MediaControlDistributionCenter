using MediaControlDistributionCenter.Helpers;
using MediaControlDistributionCenter.Helpers.FTP.Client;
using MediaControlDistributionCenter.Services.DTO.Models;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class UploadService : Proxy, IUploadService
    {
        public event EventHandler<ProgressEventArgs>? InvokeProgressChanged;

        public UploadService(ConnectionMode options) : base(options)
        {
        }

        public async Task<ResultResponse<bool>> UploadFile(string filePath, string fileName, bool hasProgress = false)
        {
            var url = hasProgress ? "/programme/upload" : "/media/upload";

            var formFile = new WpfFormFile(filePath, fileName);
            var result = await PostMultipleFiles<ResultResponse<bool>>(url, formFile);
            if (result == null)
            {
                result = ResultResponse<bool>.ErrorInstance("Response error");
            }

            return result;
        }



        public async Task<ResultResponse<bool>> DownloadFile(string fileName)
        {
            var url = "/media/download";
            url += $"?fileName={fileName}";
            try
            {
                var result = await GetAttachedFile(url);
                if (result == null)
                {
                    return new ResultResponse<bool>
                    {
                        Code = 200,
                        Data = true,
                        Message = "Fail to download ",
                    };
                }
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.OutPath, fileName);
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                await result.CopyToAsync(fileStream);
                result.Dispose();
                return new ResultResponse<bool>
                {
                    Code = 200,
                    Data = true,
                    Message = "OK",
                };
            }
            catch (Exception ex) 
            {
                Log.Error(ex.Message);
                return new ResultResponse<bool>
                {
                    Code = -1,
                    Data = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
