using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DAL.Concrete;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Check for testing environment and load test configuration if needed
if (builder.Environment.EnvironmentName == "Testing")
{
    builder.Configuration.AddJsonFile("appsettings.Testing.json", optional: true);
}

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
}

builder.Services.AddHttpClient<IOpenAIService, OpenAIService>()
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(x => (int)x.StatusCode == 429)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
// Register HttpClient for MovieService
builder.Services.AddHttpClient<IMovieService, MovieService>();
builder.Services.AddScoped<IMovieService, MovieService>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new MovieService(httpClient, configuration);
});

builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();

var azurePublish = !builder.Environment.IsDevelopment();

// Only use SQL Server if we're not in a testing environment
if (builder.Environment.EnvironmentName != "Testing")
{
    var connectionString = builder.Configuration.GetConnectionString(
        azurePublish ? "AzureConnection" : "DefaultConnection") ??
        throw new InvalidOperationException("Connection string not found.");

    var authConnectionString = builder.Configuration.GetConnectionString(
        azurePublish ? "AzureIdentityConnection" : "IdentityConnection") ??
        throw new InvalidOperationException("Identity Connection string not found.");

    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseLazyLoadingProxies().UseSqlServer(connectionString));

    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseLazyLoadingProxies().UseSqlServer(authConnectionString));
}
// For Testing environment, the test factory will provide its own DB contexts

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<UserDbContext>());

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddRazorPages();

// Add Session Services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Skip seeding test users when running in Testing environment (handled by test setup)
if (app.Environment.EnvironmentName != "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await SeedData.InitializeAsync(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// Place namespace declaration AFTER all top-level statements
namespace MoviesMadeEasy
{
    public partial class Program { }
}