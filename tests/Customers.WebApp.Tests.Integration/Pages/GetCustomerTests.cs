using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.WebApp.Tests.Integration.Pages
{
    public class GetCustomerTests
    {
        private readonly SharedTestContext _testContext;

        private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername)
            .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date));

        public GetCustomerTests(SharedTestContext testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Get_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
            {
                BaseURL = SharedTestContext.AppUrl
            });
            var customer = await CreateCustomer(page);

            // Act
            var linkElement = page.Locator("article>p>a").First;
            var link = await linkElement.GetAttributeAsync("href");
            await page.GotoAsync(link!);

            // Assert
            (await page.Locator("p[id=fullname-field]").InnerHTMLAsync()).Should().Be(customer.FullName);
            (await page.Locator("p[id=email-field]").InnerHTMLAsync()).Should().Be(customer.Email);
            (await page.Locator("p[id=github-username-field]").InnerHTMLAsync()).Should().Be(customer.GitHubUsername);
            (await page.Locator("p[id=dob-field]").InnerHTMLAsync()).Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));

            // Cleanup
            // await page.CloseAsync();
        }

        [Fact]
        public async Task Get_ReturnsNoCustomer_WhenNoCustomerExists()
        {
            // Arrange
            var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
            {
                BaseURL = SharedTestContext.AppUrl
            });
            var customerUrl = $"{SharedTestContext.AppUrl}/customer/{Guid.NewGuid()}";

            // Act
            await page.GotoAsync(customerUrl);

            // Assert
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

            return customer;
        }
    }
}
