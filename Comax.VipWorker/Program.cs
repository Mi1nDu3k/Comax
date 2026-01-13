
using Comax.Data;
using Comax.VipWorker.Jobs.Subscription;
using Comax.VipWorker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Lấy connection string
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = "Server=127.0.0.1;Database=ComaxDb;User=root;Password=11111111;";

builder.Services.AddDbContext<ComaxDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<SubscriptionWorker>();

var host = builder.Build();
host.Run();