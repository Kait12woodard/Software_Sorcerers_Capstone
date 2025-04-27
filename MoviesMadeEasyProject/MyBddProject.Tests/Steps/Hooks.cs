using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Data;
using MyBddProject.Tests.Mocks;
using MoviesMadeEasy.DAL;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private Process? _serverProcess;
        private IHost? _testHost;
        private int _testPort = 5123;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Find an available port
            _testPort = GetAvailablePort();
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
                bool isGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

                // Setup Chrome options
                var options = new ChromeOptions();
                options.AddArguments("--headless", "--disable-gpu");

                if (isGithubActions)
                {
                    options.AddArguments(
                        "--no-sandbox",
                        "--disable-dev-shm-usage",
                        "--window-size=1920,1080"
                    );
                }

                _driver = new ChromeDriver(options);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                _objectContainer.RegisterInstanceAs(_driver);

                if (isGithubActions)
                {
                    // For GitHub Actions, start a real server on a fixed port
                    StartTestServer();
                    var baseUrl = $"http://localhost:{_testPort}";
                    Console.WriteLine($"Using test server URL: {baseUrl}");

                    // Wait for app to start
                    bool isAvailable = WaitForAppAvailability(baseUrl, 30);
                    if (!isAvailable)
                    {
                        throw new Exception($"Application not available at {baseUrl}");
                    }

                    _driver.Navigate().GoToUrl(baseUrl);
                }
                else
                {
                    // For local development, start via dotnet run
                    StartApplicationServer();

                    var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                    Console.WriteLine($"Using base URL: {baseUrl}");

                    bool isAvailable = WaitForAppAvailability(baseUrl, 30);
                    if (!isAvailable)
                    {
                        throw new Exception($"Application not available at {baseUrl}");
                    }

                    _driver.Navigate().GoToUrl(baseUrl);
                }
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
            if (_driver != null)
            {
                try
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error quitting driver: {ex.Message}");
                }
            }

            StopApplicationServer();
            StopTestServer();
        }

        private void StartTestServer()
        {
            Console.WriteLine($"Starting test server on port {_testPort}...");

            _testHost = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TestStartup>();
                    webBuilder.UseUrls($"http://localhost:{_testPort}");
                })
                .Build();

            // Start the server
            _testHost.Start();
            Console.WriteLine("Test server started successfully");
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

        private void StartApplicationServer()
        {
            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project ../../../MoviesMadeEasy/MoviesMadeEasy.csproj --environment Testing",
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };
            _serverProcess.Start();
            Console.WriteLine("Started local application server process");
        }

        private void StopApplicationServer()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                try
                {
                    _serverProcess.Kill();
                    _serverProcess = null;
                    Console.WriteLine("Stopped local application server");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping server: {ex.Message}");
                }
            }
        }

        private bool WaitForAppAvailability(string url, int timeoutSeconds)
        {
            Console.WriteLine($"Checking application availability at {url}");
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            bool isSuccess = false;
            int attempts = 0;

            while (DateTime.Now < endTime && !isSuccess)
            {
                attempts++;
                try
                {
                    var response = client.GetAsync(url).Result;
                    Console.WriteLine($"Attempt {attempts}: Response {(int)response.StatusCode} {response.StatusCode}");
                    isSuccess = response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempts}: {ex.GetType().Name}");
                    Thread.Sleep(1000);
                }

                if (!isSuccess && DateTime.Now < endTime)
                {
                    Thread.Sleep(1000);
                }
            }

            return isSuccess;
        }

        private int GetAvailablePort()
        {
            // Create a new socket
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            var port = ((IPEndPoint)socket.LocalEndPoint).Port;
            socket.Close();
            return port;
        }
    }

    // Add a TestStartup class to configure the test server
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure services similar to TestWebApplicationFactory
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add in-memory database context
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("TestAuthDb"));

            // Add identity
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

            // Add mock services
            services.AddScoped<MoviesMadeEasy.DAL.Abstract.IMovieService, MockMovieService>();
            // Use direct registration without the interface to avoid namespace issues
            services.AddScoped(typeof(MoviesMadeEasy.DAL.Abstract.ISubscriptionRepository), typeof(MoviesMadeEasy.DAL.Concrete.SubscriptionRepository));
            services.AddScoped(typeof(MoviesMadeEasy.DAL.Abstract.IUserRepository), typeof(MoviesMadeEasy.DAL.Concrete.UserRepository));
            services.AddScoped(typeof(MoviesMadeEasy.DAL.Abstract.ITitleRepository), typeof(MoviesMadeEasy.DAL.Concrete.TitleRepository));

            // Register OpenAIService manually to avoid namespace issues
            services.AddScoped<OpenAIService>();
            services.AddScoped(provider => (IOpenAIService)provider.GetService<OpenAIService>());

            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Standard app configuration
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding test data: {ex}");
            }
        }
    }
}