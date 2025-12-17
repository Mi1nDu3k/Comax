using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Business.Services.Interfaces;
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
            // 2. Add DbContext
            services.AddDbContext<ComaxDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
                )
            );

            // Các Worker và Service cơ bản
            services.AddSingleton<IViewCountBuffer, ViewCountBuffer>();
            services.AddHostedService<ViewCountWorker>();

            // --- ĐĂNG KÝ STORAGE SERVICE (MINIO) ---
            // Chỉ cần dòng này là đủ. Nó sẽ map IStorageService -> MinioStorageService
            services.AddScoped<IStorageService, MinioStorageService>();

            services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // 3. Add AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // 4. Add Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IComicRepository, ComicRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // 5. Add Services
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

            // (Đã xóa IMinioService và MinioStorageService thừa ở đây đi)

            // 6. Controllers + FluentValidation
            services.AddControllers();
            services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
            // Lưu ý: Kiểm tra lại tên class BaseDto hay BaseDTO trong code của bạn
            services.AddValidatorsFromAssemblyContaining<Comax.Common.DTOs.BaseDto>();

            // 7. Swagger Configuration
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Comax API", Version = "v1" });

                // Cấu hình ổ khóa Token trên Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token vào đây. Ví dụ: Bearer {token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // 8. JWT Authentication
            services.AddScoped<IJwtHelper, JwtHelper>();

            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT Secret key is missing in configuration!");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            services.AddMemoryCache();
            services.AddAuthorization();
        }
    }
}