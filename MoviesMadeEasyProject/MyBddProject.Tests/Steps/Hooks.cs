using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using MoviesMadeEasy.DAL.Abstract;
using MyBddProject.Tests.Mocks;
using Microsoft.AspNetCore.Builder;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private IHost? _testHost;
        private int _testPort;
        private readonly string _baseUrl;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _testPort = GetAvailablePort();
            _baseUrl = $"http://localhost:{_testPort}";
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            try
            {
                Console.WriteLine("Setting up test environment...");
                StartTestServer();

                // Setup Chrome options
                var options = new ChromeOptions();

                // Configure Chrome for headless environments
                options.AddArguments("--headless", "--disable-gpu");
                options.AddArguments("--no-sandbox", "--disable-dev-shm-usage");
                options.AddArguments("--window-size=1920,1080");
                options.AddArguments("--remote-allow-origins=*");

                // Create and register the WebDriver
                _driver = new ChromeDriver(options);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(45);
                _objectContainer.RegisterInstanceAs(_driver);

                // Wait for the application to be fully available
                Console.WriteLine($"Checking application availability at {_baseUrl}");
                bool isAvailable = WaitForAppAvailability(_baseUrl, 45);
                if (!isAvailable)
                {
                    throw new Exception($"Application not available at {_baseUrl} after 45 seconds");
                }

                // Navigate to the application
                Console.WriteLine($"Navigating to {_baseUrl}");
                _driver.Navigate().GoToUrl(_baseUrl);
                Console.WriteLine($"Successfully navigated to {_baseUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SETUP ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            try
            {
                if (_driver != null)
                {
                    _driver.Quit();
                    _driver.Dispose();
                    _driver = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during driver cleanup: {ex.Message}");
            }

            StopTestServer();
        }

        private void StartTestServer()
        {
            Console.WriteLine($"Starting test server on port {_testPort}...");

            string rootDir = FindProjectRoot();
            string contentRoot = Path.Combine(rootDir, "MoviesMadeEasy");
            string wwwrootPath = Path.Combine(contentRoot, "wwwroot");

            Console.WriteLine($"Using content root: {contentRoot}");
            Console.WriteLine($"Using wwwroot path: {wwwrootPath}");

            _testHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(contentRoot);
                    webBuilder.UseWebRoot(wwwrootPath);
                    webBuilder.UseStartup<TestStartup>();
                    webBuilder.UseUrls($"http://0.0.0.0:{_testPort}"); // Bind to all interfaces, not just localhost
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, _testPort);
                    });
                })
                .Build();

            _testHost.Start();
            Console.WriteLine("Test server started successfully");
        }

        private string FindProjectRoot()
        {
            // For GitHub Actions, paths are different
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                string workDir = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ??
                                "/home/runner/work/Software_Sorcerers_Capstone/Software_Sorcerers_Capstone";

                return Path.Combine(workDir, "MoviesMadeEasyProject");
            }

            // For local development
            string currentDir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.Exists(Path.Combine(currentDir, "MoviesMadeEasy")))
                {
                    return currentDir;
                }

                var parentDir = Directory.GetParent(currentDir);
                if (parentDir == null) break;
                currentDir = parentDir.FullName;
            }

            throw new DirectoryNotFoundException("Could not find project root directory");
        }

        private void StopTestServer()
        {
            if (_testHost != null)
            {
                try
                {
                    _testHost.StopAsync().Wait();
                    _testHost.Dispose();
                    _testHost = null;
                    Console.WriteLine("Test server stopped");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping test server: {ex.Message}");
                }
            }
        }

        private bool WaitForAppAvailability(string url, int timeoutSeconds)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            int attempts = 0;

            while (DateTime.Now < endTime)
            {
                attempts++;
                try
                {
                    Console.WriteLine($"Attempt {attempts}: Checking {url}");
                    var response = client.GetAsync(url).Result;
                    Console.WriteLine($"Response: {(int)response.StatusCode} {response.StatusCode}");

                    // Even a 404 means the server is running
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempts}: {ex.GetType().Name} - {ex.Message}");

                    // Brief pause before next attempt
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        private int GetAvailablePort()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                var port = ((IPEndPoint)socket.LocalEndPoint!).Port;
                socket.Close();
                return port;
            }
            catch (Exception)
            {
                // Fallback to a fixed port if dynamic allocation fails
                return 5555;
            }
        }
    }

    // Updated TestStartup class
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Basic services
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add in-memory database contexts
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("TestAuthDb"));

            // Configure identity with simpler password requirements for testing
            services.AddDefaultIdentity<Microsoft.AspNetCore.Identity.IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IdentityDbContext>();

            // Use mock services for testing
            services.AddScoped<DbContext, UserDbContext>();
            services.AddScoped<IMovieService, MockMovieService>();
            services.AddScoped<IOpenAIService, MockOpenAIService>();
            services.AddScoped<ISubscriptionRepository, MoviesMadeEasy.DAL.Concrete.SubscriptionRepository>();
            services.AddScoped<IUserRepository, MoviesMadeEasy.DAL.Concrete.UserRepository>();
            services.AddScoped<ITitleRepository, MoviesMadeEasy.DAL.Concrete.TitleRepository>();

            // HttpClient for mock services
            services.AddHttpClient();

            // Add session support
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable developer exception page
            app.UseDeveloperExceptionPage();

            // Basic middleware pipeline
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            // Seed test data
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                MoviesMadeEasy.Data.SeedData.InitializeAsync(services).GetAwaiter().GetResult();
                Console.WriteLine("Test data seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding test data: {ex}");
            }
        }
    }
}