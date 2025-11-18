using Comax.API.Mapping;
using Comax.Business.Interfaces;
using Comax.Business.Services;
using Comax.Data;
using Comax.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container
builder.Services.AddControllers(); // Bắt buộc để API controller hoạt động
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Add DbContext (InMemory DB)
builder.Services.AddDbContext<ComaxDbContext>(options =>
    options.UseInMemoryDatabase("ComaxDb"));

// 3. Add AutoMapper
builder.Services.AddAutoMapper(typeof(ComaxMappingProfile));

// 4. Register Repositories
builder.Services.AddScoped<IComicRepository, ComicRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

// 5. Register Services
builder.Services.AddScoped<IComicService, ComicService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
//builder.Services.AddScoped<ILoadDataService, LoadDataService>();

var app = builder.Build();

// 6. Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map API Controllers
app.MapControllers();

// 7. Seed Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();
    SeedData.Initialize(dbContext);
}

app.Run();
