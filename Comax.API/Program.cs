using Comax.API.Extensions; // Nhớ using namespace này
using Comax.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // URL của Frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Nếu cần dùng Cookie/Auth header
    });
});

builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

//--- MIDDLEWARE PIPELINE --
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ComaxDbContext>();
    try
    {
        dbContext.Database.Migrate();
        DbSeeder.Seed(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

app.Run();