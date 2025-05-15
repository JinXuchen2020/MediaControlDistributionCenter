using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public class WpfFormFile : IFormFile
    {
        private readonly Stream _stream;
        private readonly string _contentType;
        private readonly string _fileName;

        public WpfFormFile(string filePath, string fileName, string contentType = "application/octet-stream")
        {
            _stream = File.OpenRead(filePath);
            _contentType = contentType;
            _fileName = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
            Length = _stream.Length;
            Name = Path.GetFileNameWithoutExtension(_fileName);
        }

        public Stream OpenReadStream()
        {
            _stream.Position = 0;
            return _stream;
        }

        public void CopyTo(Stream target)
        {
            _stream.CopyTo(target);
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            return _stream.CopyToAsync(target, cancellationToken);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public long Length { get; }
        public string ContentType => _contentType;
        public string ContentDisposition { get; set; }
        public string FileName => _fileName;
        public string Name { get; }

        public IHeaderDictionary Headers { get; set; }
    }
}
