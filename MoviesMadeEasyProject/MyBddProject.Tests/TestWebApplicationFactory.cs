using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MyBddProject.Tests.Mocks;

namespace MyBddProject.Tests;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));

            var idDbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            if (idDbContextDescriptor != null)
                services.Remove(idDbContextDescriptor);

            // Add in-memory database
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("TestAuthDb"));

            // Replace real services with mocks
            services.AddScoped<DbContext, UserDbContext>();
            services.AddScoped<IMovieService, MockMovieService>();
            services.AddScoped<IOpenAIService, MockOpenAIService>();

            // Get service provider to initialize database
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

            try
            {
                // Seed the database with test data
                SeedData.InitializeAsync(scopedServices).GetAwaiter().GetResult();
                logger.LogInformation("Database seeded with test data");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database");
            }
        });
    }
}