using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Test collection")]
public class UpdateCustomerTests
{
    private readonly SharedTestContext _testContext;
    private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
        .RuleFor(x => x.FullName, faker => faker.Person.FullName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername)
        .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date));

    public UpdateCustomerTests(SharedTestContext testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public async Task Update_UpdateCustomer_WhenDataIsValid()
    {
        // Arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });
        var customer = await CreateCustomer(page);
        await page.GotoAsync($"update-customer/{customer.Id}");
        customer.FullName = "Another Name";

        // Act
        await page.FillAsync("input[id=fullname]", customer.FullName);
        await page.ClickAsync("button[type=submit]");

        // Assert
        var linkElement = page.Locator("article>p>a").First;
        var link = await linkElement.GetAttributeAsync("href");
        await page.GotoAsync(link!);

        (await page.Locator("p[id=fullname-field]").InnerHTMLAsync()).Should().Be(customer.FullName);
        (await page.Locator("p[id=email-field]").InnerHTMLAsync()).Should().Be(customer.Email);
        (await page.Locator("p[id=github-username-field]").InnerHTMLAsync()).Should().Be(customer.GitHubUsername);
        (await page.Locator("p[id=dob-field]").InnerHTMLAsync()).Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));
    }

    [Fact]
    public async Task Update_ShowsError_WhenEmailIsInvalid()
    {
        // Arrange
        var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = SharedTestContext.AppUrl
        });
        var customer = await CreateCustomer(page);
        await page.GotoAsync($"update-customer/{customer.Id}");
        customer.Email = "invalidemail";

        // Act
        await page.FillAsync("input[id=email]", customer.FullName);
        await page.FocusAsync("input[id=fullname]");

        // Assert
        var element = page.Locator("li.validation-message").First;
        var text = await element.InnerHTMLAsync();
        text.Should().Be("Invalid email format");
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
