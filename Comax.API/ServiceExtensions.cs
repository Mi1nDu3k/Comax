using Comax.Business.Services;
using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Validators;
using Comax.Common.Helpers;
using Comax.Data;
using Comax.Data.Repositories;
using Comax.Data.Repositories.Interfaces;
using Comax.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace Comax.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ComaxDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // 2. Redis Cache (Chuyển từ Program.cs sang đây)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
                options.InstanceName = "Comax_";
            });
            // Thêm Distributed Memory Cache phòng hờ nếu Redis chết hoặc chưa cài
            // services.AddDistributedMemoryCache(); 

            // 3. SignalR
            services.AddSignalR();

            // 4. Repositories (SCOPED - Quan trọng)
            services.AddScoped<IUnitOfWork, UnitOfWork>(); // <--- Fix lỗi concurrency
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IComicRepository, ComicRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();

            // 5. Services (SCOPED)
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IComicService, ComicService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IStorageService, MinioStorageService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IEmailService, EmailService>();
           
            // 6. Workers (Singleton)
            services.AddSingleton<IViewCountBuffer, ViewCountBuffer>();
            services.AddHostedService<ViewCountWorker>();
            services.AddHostedService<Comax.API.Workers.ComicTrashCleanupWorker>();

            // 7. AutoMapper & Validators
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<Comax.Common.DTOs.BaseDto>();

            // 8. Controllers & JSON Config (Gộp vào đây)
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // 9. Auth & Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Comax API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token: Bearer {token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddScoped<IJwtHelper, JwtHelper>();
            var jwtSettings = configuration.GetSection("Jwt");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"]; // Lấy token từ URL
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                             path.StartsWithSegments("/hubs/notification")) 
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMemoryCache();
            services.AddAuthorization();
            services.AddResponseCaching();
        }
    }
}