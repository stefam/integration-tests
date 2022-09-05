using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Customers.Api.Tests.Integration.CustomerController;

public class GetAllCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _httpClient;

    private readonly Faker<CustomerRequest> _customerGenerator =
        new Faker<CustomerRequest>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGitHubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public GetAllCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsAllCustomers_WhenCustomersExist()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdCustomerResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _httpClient.GetAsync("customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        customerResponse!.Customers.Single().Should().BeEquivalentTo(createdCustomer);

        // Cleanup
        await _httpClient.DeleteAsync($"customers/{createdCustomer!.Id}");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyResult_WhenNoCustomersExist()
    {
        // Act
        var response = await _httpClient.GetAsync("customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        customerResponse!.Customers.Should().BeEmpty();
    }
}
