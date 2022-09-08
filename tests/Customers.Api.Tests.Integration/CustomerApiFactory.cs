using Customers.Api.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Customers.Api.Tests.Integration
{
    public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        public const string ValidGitHubUser = "validuser";
        public const string ThrottledUser = "throttle";

        private readonly TestcontainerDatabase _dbContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "db",
                Username = "course",
                Password = "whatever"
            })
            .Build();

        private readonly GitHubApiServer _gitHubApiServer = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove all background running services from doing undesired changes.
                services.RemoveAll(typeof(IHostedService));

                services.RemoveAll(typeof(IDbConnectionFactory));
                services.AddSingleton<IDbConnectionFactory>(_ => 
                    new NpgsqlConnectionFactory(_dbContainer.ConnectionString));

                services.AddHttpClient("GitHub", httpClient =>
                {
                    httpClient.BaseAddress = new Uri(_gitHubApiServer.Url);
                    httpClient.DefaultRequestHeaders.Add(
                        HeaderNames.Accept, "application/vnd.github.v3+json");
                    httpClient.DefaultRequestHeaders.Add(
                        HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
                });
            });
        }

        public async Task InitializeAsync()
        {
            _gitHubApiServer.Start();
            _gitHubApiServer.SetupUser(ValidGitHubUser);
            _gitHubApiServer.SetupThrottledUserUser(ThrottledUser);
            await _dbContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            _gitHubApiServer.Dispose();
        }

        //private readonly TestcontainersContainer _dbContainer =
        //    new TestcontainersBuilder<TestcontainersContainer>()
        //        .WithImage("postgres:latest")
        //        .WithEnvironment("POSTGRES_USER", "course")
        //        .WithEnvironment("POSTGRES_PASSWORD", "changeme")
        //        .WithEnvironment("POSTGRES_DB", "mydb")
        //        .WithPortBinding(5555, 5432)
        //        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        //        .Build();
    }
}
