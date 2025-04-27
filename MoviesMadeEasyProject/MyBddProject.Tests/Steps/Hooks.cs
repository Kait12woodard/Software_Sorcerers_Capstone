using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private TestWebApplicationFactory _factory = null!;
        private IServiceScope _serviceScope = null!;
        private HttpClient _client = null!;
        private Process? _serverProcess;
        private string _baseUrl = "http://localhost:5000";

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
                StartApplicationServer();

                var options = new ChromeOptions();
                // ALWAYS run headless
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

                bool isAvailable = WaitForAppAvailability(_baseUrl, 30);
                if (!isAvailable)
                {
                    throw new Exception($"Application not available at {_baseUrl}");
                }

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

            _serviceScope?.Dispose();
            _factory?.Dispose();
            StopApplicationServer();
        }

        private void StartApplicationServer()
        {
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                _factory = new TestWebApplicationFactory();
                _serviceScope = _factory.Services.CreateScope();
                _client = _factory.CreateClient();
                Console.WriteLine($"Started test server with client base address: {_client.BaseAddress}");

                // Force server to start
                var response = _client.GetAsync("/").Result;
                Console.WriteLine($"Initial ping response: {response.StatusCode}");
            }
            else
            {
                _serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --project ../../../MoviesMadeEasy/MoviesMadeEasy.csproj --environment Testing --urls http://localhost:5000",
                        UseShellExecute = true,
                        CreateNoWindow = false
                    }
                };
                _serverProcess.Start();
                Console.WriteLine("Started local application server process");
            }
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

            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" && _client != null)
            {
                try
                {
                    var response = _client.GetAsync("/").Result;
                    Console.WriteLine($"Factory client response: {(int)response.StatusCode} {response.StatusCode}");
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Factory client error: {ex.Message}");
                    return false;
                }
            }

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
    }
}