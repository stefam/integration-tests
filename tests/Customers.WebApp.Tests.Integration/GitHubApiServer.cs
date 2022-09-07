using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Customers.WebApp.Tests.Integration;

public class GitHubApiServer : IDisposable
{
    private WireMockServer _server;
    public string Url => _server.Url!;

    public void Start()
    {
        _server = WireMockServer.Start(9850);
    }

    public void SetupUser(string userName)
    {
        _server.Given(
            Request.Create()
                .WithPath($"/users/{userName}")
                .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(GenerateGitHubUserResponseBody(userName))
                        .WithHeader("content-type", "application/json; charset=utf-8")
                        .WithStatusCode(200));
    }

    public void SetupThrottledUserUser(string userName)
    {
        _server.Given(
            Request.Create()
                .WithPath($"/users/{userName}")
                .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(@"{
                            ""message"": ""API rate limit exceeded for 127.0.0.1. (But here's the good news: Authenticated requests get a higher rate limit. Check out the documentation for more details.)"",
                            ""documentation_url"": ""https://docs.github.com/rest/overview/resources-in-the-rest-api#rate-limiting""
                        }")
                        .WithHeader("content-type", "application/json; charset=utf-8")
                        .WithStatusCode(403));
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private static string GenerateGitHubUserResponseBody(string userName)
    {
        return $@"{{
          ""login"": ""{userName}"",
          ""id"": 12499171,
          ""node_id"": ""MDQ6VXNlcjEyNDk5MTcx"",
          ""avatar_url"": ""https://avatars.githubusercontent.com/u/12499171?v=4"",
          ""gravatar_id"": "",
          ""url"": ""https://api.github.com/users/stefam"",
          ""html_url"": ""https://github.com/stefam"",
          ""followers_url"": ""https://api.github.com/users/{userName}/followers"",
          ""following_url"": ""https://api.github.com/users/{userName}/following{{/other_user}}"",
          ""gists_url"": ""https://api.github.com/users/{userName}/gists{{/gist_id}}"",
          ""starred_url"": ""https://api.github.com/users/{userName}/starred{{/owner}}{{/repo}}"",
          ""subscriptions_url"": ""https://api.github.com/users/{userName}/subscriptions"",
          ""organizations_url"": ""https://api.github.com/users/{userName}/orgs"",
          ""repos_url"": ""https://api.github.com/users/{userName}/repos"",
          ""events_url"": ""https://api.github.com/users/{userName}/events{{/privacy}}"",
          ""received_events_url"": ""https://api.github.com/users/{userName}/received_events"",
          ""type"": ""User"",
          ""site_admin"": false,
          ""name"": null,
          ""company"": null,
          ""blog"": "",
          ""location"": null,
          ""email"": null,
          ""hireable"": null,
          ""bio"": null,
          ""twitter_username"": null,
          ""public_repos"": 9,
          ""public_gists"": 0,
          ""followers"": 0,
          ""following"": 1,
          ""created_at"": ""2017 - 10 - 21T17: 33:20Z"",
          ""updated_at"": ""2022 - 08 - 30T23: 46:02Z""
        }}";
    }
}
