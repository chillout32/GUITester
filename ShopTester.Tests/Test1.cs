using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PlaywrightTests;

[DoNotParallelize]
[TestClass]
public class DemoTest : PageTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _browserContext;
    private IPage _page;

    private string GenerateUniqueString(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }


    [TestInitialize]
    public async Task Setup()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 1000 // Lägger in en fördröjning så vi kan se vad som händer
        });
        _browserContext = await _browser.NewContextAsync();
        _page = await _browserContext.NewPageAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _browserContext.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [TestMethod]
    public async Task GetShopLink()
    {
        await _page.GotoAsync("http://localhost:5173/");

        await Expect(_page.Locator("h2", new() { HasTextString = "All our partners" }))
            .ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Register()
    {
        var uniqueEmail = $"{GenerateUniqueString("user")}@example.com";
        var password = "TestPassword123";
        var uniqueUsername = GenerateUniqueString("TestUser");
        var uniqueCompany = GenerateUniqueString("TestCompany");

        await _page.GotoAsync("http://localhost:5173/");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();

        //Få programmet och se 4 input fields så den vet att den äör på rätt sida
        var inputFields = _page.Locator("input");
        await Expect(inputFields).ToHaveCountAsync(4);

        await _page.FillAsync("input[name='email']", uniqueEmail);
        await _page.FillAsync("input[name='password']", password);
        await _page.FillAsync("input[name='username']", uniqueUsername);
        await _page.FillAsync("input[name='company']", uniqueCompany);

        _page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        await _page.GetByRole(AriaRole.Button, new() { Name = "Skapa Konto" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync("http://localhost:5173/login");
    }


    [TestMethod]
    public async Task LoginAsAdmin()
    {
        await _page.GotoAsync("http://localhost:5173/login");

        var inputFields = _page.Locator("input");
        await Expect(inputFields).ToHaveCountAsync(2);

        await _page.FillAsync("input[name='email']", "m@email.com");
        await _page.FillAsync("input[name='password']", "abc123");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(_page).ToHaveURLAsync("http://localhost:5173");
    }


    [TestMethod]
    public async Task LoginAsUser()
    {
        await _page.GotoAsync("http://localhost:5173/login");

        var inputFields = _page.Locator("input");
        await Expect(inputFields).ToHaveCountAsync(2);

        await _page.FillAsync("input[name='email']", "no@email.com");
        await _page.FillAsync("input[name='password']", "abc123");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(_page).ToHaveURLAsync("http://localhost:5173");
    }


    [TestMethod]
    public async Task ChangeTicketState()
    {
        LoginAsAdmin();

        await _page.GetByRole(AriaRole.Link, new() { Name = "Issues" }).ClickAsync();

        await Expect(_page).ToHaveURLAsync("http://localhost:5173/employee/issues");

        await Expect(_page.Locator("div.list")).ToBeVisibleAsync();
        await _page.ClickAsync("button.subjectEditButton");

        await _page.SelectOptionAsync("select.stateSelect", "CLOSED");

        await _page.ClickAsync("button.stateUpdateButton");

    }
}