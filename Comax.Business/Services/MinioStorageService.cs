using Comax.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IConfiguration _config;
        private readonly IMinioClient _minioClient;

        // KHAI BÁO THÊM CÁC BIẾN CẦN THIẾT
        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly bool _useSSL;

        public MinioStorageService(IConfiguration config)
        {
            // 1. Sửa lỗi cú pháp: Đưa lệnh gán vào trong ngoặc nhọn
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // 2. Lấy giá trị từ Config gán cho biến toàn cục
            _endpoint = _config.GetValue<string>("Minio:Endpoint");
            var accessKey = _config.GetValue<string>("Minio:AccessKey");
            var secretKey = _config.GetValue<string>("Minio:SecretKey");
            _bucketName = _config.GetValue<string>("Minio:BucketName");
            _useSSL = _config.GetValue<bool>("Minio:UseSSL"); // Mặc định false nếu không có

            // 3. Khởi tạo Minio Client
            if (!string.IsNullOrEmpty(_endpoint))
            {
                var builder = new MinioClient()
                                .WithEndpoint(_endpoint)
                                .WithCredentials(accessKey, secretKey);

                // Cấu hình SSL dựa trên config
                if (_useSSL)
                {
                    builder.WithSSL();
                }

                _minioClient = builder.Build();
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            // Kiểm tra kết nối trước
            if (_minioClient == null) throw new InvalidOperationException("Minio Client chưa được khởi tạo.");

            // 1. Kiểm tra và tạo Bucket nếu chưa có
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);

                // Set policy public để xem ảnh trên web
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
            var newFileName = $"{folderName}/{Guid.NewGuid()}{fileExtension}";

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
            // Lưu ý: _endpoint thường là localhost:9000
            return $"{protocol}://{_endpoint}/{_bucketName}/{newFileName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl) || _minioClient == null) return;

            try
            {
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath.Trim('/');
                // path ví dụ: bucket-name/comics/abc.jpg

                // Cắt bỏ phần bucket name để lấy object key
                // Cách an toàn hơn để lấy Object Name:
                var objectName = path.Replace($"{_bucketName}/", "");

                var args = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(args);
            }
            catch
            {
                // Log lỗi nếu cần
            }
        }
    }
}