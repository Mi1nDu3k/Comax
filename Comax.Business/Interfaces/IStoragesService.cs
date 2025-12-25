using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface IStorageService
    {
      
        Task<string> UploadFileAsync(IFormFile file, string folderName);

        Task DeleteFileAsync(string fileName);
    }
}