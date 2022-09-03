using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Customers.Api.Tests.Integration.CustomerController;

// [CollectionDefinition("CustomerApi Collection")]
public class GetCustomerControllerTests : IClassFixture<CustomerApiFactory>, IAsyncLifetime
{
    // private readonly CustomerApiFactory _apiFactory;

    private readonly HttpClient _httpClient;

    private readonly Faker<CustomerRequest> _customerGenerator =
        new Faker<CustomerRequest>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGitHubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    private readonly List<Guid> _createdIds = new();

    public GetCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        // _apiFactory = apiFactory;
        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdCustomerResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _httpClient.GetAsync($"customers/{createdCustomer!.Id}");

        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        //response.Headers.Location!.ToString().Should()
        //    .Be($"http://localhost/customers/{customerResponse!.Id}");

        _createdIds.Add(customerResponse.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Act
        var response = await _httpClient.GetAsync($"customers/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync()
    {
        //for (int i = 0; i < 4; i++)
        //{
        //    var customer = _customerGenerator.Generate();
        //    var response = await _httpClient.PostAsJsonAsync("customers", customer);
        //    var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        //    _createdIds.Add(customerResponse!.Id);
        //}

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var createdId in _createdIds)
        {
            await _httpClient.DeleteAsync($"customers/{createdId}");
        }
    }
}
