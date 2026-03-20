
using ClawFlgma.AuthService.Data;
using ClawFlgma.AuthService.Extensions;
using ClawFlgma.AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


namespace ClawFlgma.AuthService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddApplicationDbContext(builder.Configuration);
        builder.Services.AddApplicationServices(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapGet("/scalar/{documentName}", (string documentName) =>
            {
                return Results.Redirect($"/scalar/v1/{documentName}");
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var seedData = scope.ServiceProvider.GetRequiredService<SeedDataService>();
            await seedData.EnsureSeedDataAsync();
        }

        app.Run();
    }
}

