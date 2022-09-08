using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Test collection")]
public class DeleteCustomerTests
{
    private readonly SharedTestContext _testContext;
    private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername)
        .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date));

    public DeleteCustomerTests(SharedTestContext testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public async Task Delete_DeletesCustomer_WhenCustomerExists()
    {
        // Arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });
        var customer = await CreateCustomer(page);
        await page.GotoAsync($"customers/{customer.Id}");

        // Act
        page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await page.ClickAsync("button.btn.btn-danger");

        // Assert
        await page.GotoAsync($"customers/{customer.Id}");
        (await page.Locator("article>p").InnerHTMLAsync())
            .Should().Be("No customer found with this id");
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

        var element = page.Locator("article>p>a").First;
        var link = await element!.GetAttributeAsync("href");
        var idInText = link!.Split('/').Last();
        customer.Id = Guid.Parse(idInText);

        return customer;
    }
}
