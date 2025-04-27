using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IWebDriver? _driver;
        private readonly IConfiguration _configuration;
        private Process? _serverProcess;

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
                bool isGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

                // Setup ChromeDriver - HEADLESS MODE FOR BOTH ENVIRONMENTS
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

                // Different approach for GitHub Actions vs local
                if (isGithubActions)
                {
                    // For GitHub Actions, use the application factory (in-process testing)
                    var factory = new TestWebApplicationFactory();
                    _objectContainer.RegisterInstanceAs(factory);
                    var client = factory.CreateClient();

                    // Use the factory client's base address
                    var baseUrl = client.BaseAddress?.ToString() ?? "http://localhost";
                    Console.WriteLine($"Using base URL from factory: {baseUrl}");
                    _driver.Navigate().GoToUrl(baseUrl);
                }
                else
                {
                    // For local development, start the actual server as a process
                    StartApplicationServer();

                    // Get the base URL from configuration
                    var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                    Console.WriteLine($"Using base URL: {baseUrl}");

                    // Wait for app to start
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

            // Dispose the factory if it was registered
            if (_objectContainer.IsRegistered<TestWebApplicationFactory>())
            {
                _objectContainer.Resolve<TestWebApplicationFactory>().Dispose();
            }
        }

        private void StartApplicationServer()
        {
            // Start the actual server process locally
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
    }
}