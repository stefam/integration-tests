using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Api.Tests.Integration.CustomerController;

public class UpdateCustomerControllerTests : IClassFixture<CustomerApiFactory>
{
    private readonly HttpClient _httpClient;

    private readonly Faker<CustomerRequest> _customerGenerator =
        new Faker<CustomerRequest>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGitHubUser)
        .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

    public UpdateCustomerControllerTests(CustomerApiFactory apiFactory)
    {
        _httpClient = apiFactory.CreateClient();
    }

    [Fact]
    public async Task Update_UpdatesUser_WhenDataIsValid()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdCustomerResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        customer = _customerGenerator.Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(customer);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ReturnsValidationError_WhenEmailIsInvalid()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdCustomerResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        const string invalidEmail = "aasdf3245";
        customer = _customerGenerator.Clone()
            .RuleFor(x => x.Email, invalidEmail).Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
    }

    [Fact]
    public async Task Update_ReturnsValidationError_WhenGitHubUserDoestNotExist()
    {
        // Arrange
        var customer = _customerGenerator.Generate();
        var createdCustomerResponse = await _httpClient.PostAsJsonAsync("customers", customer);
        var createdCustomer = await createdCustomerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        const string invalidGitHubUser = "435sdfa";
        customer = _customerGenerator.Clone()
            .RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

        // Act
        var response = await _httpClient.PutAsJsonAsync($"customers/{createdCustomer!.Id}", customer);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        error!.Status.Should().Be(400);
        error.Title.Should().Be("One or more validation errors occurred.");
        error.Errors["GitHubUsername"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
    }
}
