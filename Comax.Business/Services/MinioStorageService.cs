using Comax.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args; // Lưu ý namespace này ở các bản Minio mới
using System;
using System.IO;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly bool _useSSL;

        public MinioStorageService(IConfiguration config)
        {
            _endpoint = config["Minio:Endpoint"];
            var accessKey = config["Minio:AccessKey"];
            var secretKey = config["Minio:SecretKey"];
            _bucketName = config["Minio:BucketName"];
            _useSSL = bool.Parse(config["Minio:UseSSL"]);

            _minioClient = new MinioClient()
                .WithEndpoint(_endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(_useSSL)
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            // 1. Kiểm tra và tạo Bucket nếu chưa có
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);

                // Set policy public (nếu cần ảnh xem được trực tiếp trên web)
                // Đây là bước quan trọng nếu muốn link ảnh chạy được trên thẻ <img src="...">
                var policyJson = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{""AWS"": [""*""]}},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                    }}]
                }}";
                await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(_bucketName).WithPolicy(policyJson));
            }

            // 2. Tạo tên file unique
            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"{folderName}/{Guid.NewGuid()}{fileExtension}"; // VD: comics/abc-xyz.jpg

            // 3. Upload
            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(newFileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // 4. Trả về URL công khai
            var protocol = _useSSL ? "https" : "http";
            return $"{protocol}://{_endpoint}/{_bucketName}/{newFileName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            // Parse lấy object name từ URL
            // URL dạng: http://localhost:9000/bucket/comics/abc.jpg
            // Object name: comics/abc.jpg
            try
            {
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath.Trim('/'); // bucket/comics/abc.jpg
                var objectName = path.Substring(path.IndexOf('/') + 1); // comics/abc.jpg

                var args = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(args);
            }
            catch
            {
                // Log lỗi nếu cần, hoặc bỏ qua
            }
        }
    }
}