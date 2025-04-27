using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MyBddProject.Tests.Mocks;
using System.Collections.Generic;

namespace MyBddProject.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<MoviesMadeEasy.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                // Add test configuration settings
                var inMemorySettings = new Dictionary<string, string>
                {
                    {"OpenAI_ApiKey", "sk-dummy-key-for-testing"},
                    {"RapidApiKey", "dummy-key-for-testing"},
                    {"OpenAI_Model", "gpt-3.5-turbo"}
                };
                config.AddInMemoryCollection(inMemorySettings!);
            });

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove existing database contexts
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var identityDbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
                if (identityDbContextDescriptor != null)
                    services.Remove(identityDbContextDescriptor);

                // Add in-memory database for testing
                services.AddDbContext<UserDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                services.AddDbContext<IdentityDbContext>(options =>
                    options.UseInMemoryDatabase("TestAuthDb"));

                // Mock services
                services.AddScoped<IMovieService, MockMovieService>();
                services.AddScoped<IOpenAIService, MockOpenAIService>();

                // Simplified password requirements for testing
                services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                });

                // Ensure databases are created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var userDb = scopedServices.GetRequiredService<UserDbContext>();
                var identityDb = scopedServices.GetRequiredService<IdentityDbContext>();
                userDb.Database.EnsureCreated();
                identityDb.Database.EnsureCreated();

                // Seed test data
                try
                {
                    MoviesMadeEasy.Data.SeedData.InitializeAsync(scopedServices).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding test data: {ex.Message}");
                }
            });
        }
    }
}