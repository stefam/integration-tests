using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

var wiremockServer = WireMockServer.Start();

Console.WriteLine($"Wiremock is now running on: {wiremockServer.Url}");

wiremockServer.Given(
    Request.Create()
    .WithPath("/users/stefam")
    .UsingGet())
    .RespondWith(
    Response.Create()
        .WithBodyAsJson(@"{
          ""login"": ""stefam"",
          ""id"": 12499171,
          ""node_id"": ""MDQ6VXNlcjEyNDk5MTcx"",
          ""avatar_url"": ""https://avatars.githubusercontent.com/u/12499171?v=4"",
          ""gravatar_id"": "",
          ""url"": ""https://api.github.com/users/stefam"",
          ""html_url"": ""https://github.com/stefam"",
          ""followers_url"": ""https://api.github.com/users/stefam/followers"",
          ""following_url"": ""https://api.github.com/users/stefam/following{/other_user}"",
          ""gists_url"": ""https://api.github.com/users/stefam/gists{/gist_id}"",
          ""starred_url"": ""https://api.github.com/users/stefam/starred{/owner}{/repo}"",
          ""subscriptions_url"": ""https://api.github.com/users/stefam/subscriptions"",
          ""organizations_url"": ""https://api.github.com/users/stefam/orgs"",
          ""repos_url"": ""https://api.github.com/users/stefam/repos"",
          ""events_url"": ""https://api.github.com/users/stefam/events{/privacy}"",
          ""received_events_url"": ""https://api.github.com/users/stefam/received_events"",
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
        }")
        .WithHeader("content-type", "application/json; charset=utf-8")
        .WithStatusCode(200));

//wiremockServer.Given(
//    Request.Create()
//    .WithPath("/example")
//    .UsingGet())
//    .RespondWith(
//    Response.Create()
//        .WithBody("This is coming from WireMock")
//        .WithStatusCode(200));

Console.ReadKey();
wiremockServer.Dispose();