using Comax.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Comax.Business.Services
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
                // A. Tạo Bucket mới
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);

                // B. [MỚI] Tự động set Policy là PUBLIC để sửa lỗi 403 Forbidden
                string policyJson = $@"{{
                  ""Version"": ""2012-10-17"",
                  ""Statement"": [
                    {{
                      ""Effect"": ""Allow"",
                      ""Principal"": {{ ""AWS"": [""*""] }},
                      ""Action"": [""s3:GetObject""],
                      ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                    }}
                  ]
                }}";

                await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                    .WithBucket(bucketName)
                    .WithPolicy(policyJson));
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

            // 3. Trả về đường dẫn
            // Ví dụ: http://localhost:9000/comics-bucket/abc.jpg
            var minioUrl = _configuration["Minio:DisplayUrl"] ?? _configuration["Minio:Endpoint"];
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