using Comax.Crawler;
using Comax.Data;
using Microsoft.EntityFrameworkCore;
using Minio;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;

        // 1. Đăng ký HttpClient
        services.AddHttpClient();

        // 2. Đăng ký Database (MySQL)
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<ComaxDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        // 3. Đăng ký MinIO Client (Singleton)
        services.AddSingleton<IMinioClient>(sp =>
        {
            return new MinioClient()
                .WithEndpoint(config["Minio:Endpoint"])
                .WithCredentials(config["Minio:AccessKey"], config["Minio:SecretKey"])
                .WithSSL(config.GetValue<bool>("Minio:UseSSL"))
                .Build();
        });

        // 4. Đăng ký Worker (Con bot)
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();