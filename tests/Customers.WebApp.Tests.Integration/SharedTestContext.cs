using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Services;
using Microsoft.Playwright;

namespace Customers.WebApp.Tests.Integration;

public class SharedTestContext : IAsyncLifetime
{
    public const string ValidGitHubUsername = "stefam";
    public const string AppUrl = "https://localhost:7780";

    public GitHubApiServer GitHubApiServer { get; } = new();

    private static readonly string DockerComposeFile =
        Path.Combine(Directory.GetCurrentDirectory(), (TemplateString)"../../../docker-compose-integration.yml");

    private readonly ICompositeService _dockerService =
        new Builder()
        .UseContainer()
        .UseCompose()
        .FromFile(DockerComposeFile)
        .RemoveOrphans()
        .WaitForHttp("test-app", AppUrl)
        .Build();

    private IPlaywright _playwright;

    public IBrowser Browser { get; private set; }

    public async Task InitializeAsync()
    {
        GitHubApiServer.Start();
        GitHubApiServer.SetupUser(ValidGitHubUsername);
        _dockerService.Start();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            //Headless = false,
            SlowMo = 150
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        _dockerService.Dispose();
        GitHubApiServer.Dispose();
    }
}