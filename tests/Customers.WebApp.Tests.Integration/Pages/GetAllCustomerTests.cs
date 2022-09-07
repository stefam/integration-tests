using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Test collection")]
public class GetAllCustomerTests
{
    private readonly SharedTestContext _testContext;
    private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername)
        .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date));

    public GetAllCustomerTests(SharedTestContext testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public async Task GetAll_ContainsCustomer_WhenCustomerExists()
    {
        // Arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });
        var customer = await CreateCustomer(page);

        // Act
        await page.GotoAsync("/customers");

        var name = page.Locator("tbody>tr>td")
            .Filter(new LocatorFilterOptions { HasTextString = customer.FullName });

        // Assert
        (await name.First.InnerTextAsync()).Should().Be(customer.FullName);

        // Cleanup
        // await page.CloseAsync();
    }

    private async Task<Customer> CreateCustomer(IPage page)
    {
        await page.GotoAsync("add-customer");
        var customer = _customerGenerator.Generate();

        await page.FillAsync("input[id=fullname]", customer.FullName);
        await page.FillAsync("input[id=email]", customer.Email);
        await page.FillAsync("input[id=github-username]", customer.GitHubUsername);
        await page.FillAsync("input[id=dob]", customer.DateOfBirth.ToString("yyyy-MM-dd"));

        await page.ClickAsync("button[type=submit]");

        return customer;
    }
}
