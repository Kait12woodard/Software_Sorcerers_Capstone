// Hooks.cs
using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private TestWebApplicationFactory? _factory;
        private IServiceScope? _serviceScope;
        private HttpClient? _client;
        private Process? _serverProcess;
        private readonly string _baseUrl = "http://localhost:5000";

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
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
                // Always use TestWebApplicationFactory for consistency between environments
                _factory = new TestWebApplicationFactory();
                _serviceScope = _factory.Services.CreateScope();
                _client = _factory.CreateClient();

                // Warm up server, very important!
                Console.WriteLine($"Started test server with client base address: {_client.BaseAddress}");
                var warmUpResponse = _client.GetAsync("/").Result;
                Console.WriteLine($"Initial ping response: {warmUpResponse.StatusCode}");

                // Ensure server is ready before proceeding
                bool isAvailable = WaitForAppAvailability(_baseUrl, 30);
                if (!isAvailable)
                {
                    throw new Exception($"Application not available at {_baseUrl}");
                }

                // Setup ChromeDriver (always headless)
                var options = new ChromeOptions();
                options.AddArguments(
                    "--headless",
                    "--disable-gpu",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--window-size=1920,1080"
                );

                _driver = new ChromeDriver(options);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                _objectContainer.RegisterInstanceAs(_driver);

                // Navigate to the app
                Console.WriteLine($"Navigating to {_baseUrl}");
                _driver.Navigate().GoToUrl(_baseUrl);
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error quitting WebDriver: {ex.Message}");
            }

            try
            {
                _serviceScope?.Dispose();
                _factory?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing factory: {ex.Message}");
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
                    Console.WriteLine($"Attempt {attempts}: {ex.GetType().Name} - {ex.Message}");
                    Thread.Sleep(1000);
                }

                if (!isSuccess && DateTime.Now < endTime)
                {
                    Thread.Sleep(1000);
                }
            }

            return isSuccess;
        }
    }
}