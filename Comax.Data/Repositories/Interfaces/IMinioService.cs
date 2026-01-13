using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services.Interfaces
{
    public interface IMinioService
    {

        Task<string> UploadFileAsync(IFormFile file, string bucketName);

        Task<List<string>> UploadFilesAsync(List<IFormFile> files, string bucketName);
    }
}