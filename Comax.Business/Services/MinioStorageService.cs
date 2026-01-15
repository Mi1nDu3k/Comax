using Comax.Business.Interfaces;
using Comax.Common.Constants;
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
        // SỬA LỖI 1: Phải dùng IMinioClient (Interface) thay vì MinioClient
        private readonly IMinioClient _minioClient;

        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly bool _useSSL;

        public MinioStorageService(IConfiguration config)
        {
            var conf = config ?? throw new ArgumentNullException(nameof(config));
            _endpoint = conf.GetValue<string>("Minio:Endpoint");
            var accessKey = conf.GetValue<string>("Minio:AccessKey");
            var secretKey = conf.GetValue<string>("Minio:SecretKey");
            _bucketName = conf.GetValue<string>("Minio:BucketName");
            _useSSL = conf.GetValue<bool>("Minio:UseSSL");

            if (!string.IsNullOrEmpty(_endpoint))
            {
                var builder = new MinioClient()
                                .WithEndpoint(_endpoint)
                                .WithCredentials(accessKey, secretKey);
                if (_useSSL) builder.WithSSL();

                // Builder trả về IMinioClient, nên biến _minioClient phải là IMinioClient
                _minioClient = builder.Build();
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (_minioClient == null) throw new InvalidOperationException(SystemMessages.Common.MinioNotInit);

            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"{folderName}/{Guid.NewGuid()}{fileExtension}";

            using (var stream = file.OpenReadStream())
            {
                // SỬA LỖI 2: Dùng PutObjectArgs thay vì truyền 5 tham số
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(newFileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            //var protocol = _useSSL ? "https" : "http";
            return newFileName;
        }

        public async Task DeleteFileAsync(string objectName) // Đổi tên tham số cho rõ nghĩa
        {
            if (string.IsNullOrEmpty(objectName) || _minioClient == null) return;

            try
            {
               
                var removeArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeArgs);
            }
            catch (Exception ex)
            {
                // Log _logger.LogError(ex.Message);
            }
        }
    }
    }