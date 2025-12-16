using Comax.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;
using Comax.Business.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Comax.Infrastructure.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;

        public MinioService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Lấy cấu hình từ appsettings.json
            var endpoint = _configuration["Minio:Endpoint"];
            var accessKey = _configuration["Minio:AccessKey"];
            var secretKey = _configuration["Minio:SecretKey"];

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false) // Đặt false nếu chạy localhost http
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bucketName)
        {
            // 1. Kiểm tra và tạo Bucket nếu chưa có
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }

            // 2. Upload File
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // 3. Trả về đường dẫn (Cần cấu hình đúng URL hiển thị)
            // Ví dụ: http://localhost:9000/comics-bucket/abc.jpg
            var minioUrl = _configuration["Minio:DisplayUrl"] ?? _configuration["Minio:Endpoint"];
            // Lưu ý: Nếu chạy Docker, Endpoint có thể là 'minio:9000' nhưng DisplayUrl phải là 'localhost:9000'
            return $"http://{minioUrl}/{bucketName}/{fileName}";
        }

        public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, string bucketName)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                var url = await UploadFileAsync(file, bucketName);
                urls.Add(url);
            }
            return urls;
        }
    }
}