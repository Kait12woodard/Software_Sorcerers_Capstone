using Microsoft.Extensions.Configuration;
using MoviesMadeEasy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using Reqnroll.BoDi;

namespace MyBddProject.Tests.Steps;

[Binding]
public class Hooks
{
    private readonly IObjectContainer _objectContainer;
    private readonly IConfiguration _configuration;
    private IWebDriver _driver;
    private TestWebApplicationFactory<Program> _factory;

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
        // Create WebApplicationFactory
        _factory = new TestWebApplicationFactory<Program>();
        _objectContainer.RegisterInstanceAs(_factory);

        // Initialize ChromeDriver
        var options = new ChromeOptions();

        if (IsRunningInGitHubActions())
        {
            options.AddArguments(
                "--headless",
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--remote-allow-origins=*",
                "--window-size=1920,1080"
            );
        }
        else
        {
            // Local testing options
            options.AddArguments("--headless", "--disable-gpu");
        }

        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        _objectContainer.RegisterInstanceAs(_driver);

        // Start test server using factory and navigate to base URL
        string baseUrl = _factory.Server.BaseAddress.ToString();
        Console.WriteLine($"Using test server URL: {baseUrl}");

        _driver.Navigate().GoToUrl(baseUrl);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _driver?.Quit();
        _driver?.Dispose();
        _factory?.Dispose();
    }

    private bool IsRunningInGitHubActions()
    {
        return Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
    }
}