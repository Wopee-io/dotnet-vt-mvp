using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using PlaywrightTests.Fixtures;
using PlaywrightTests.GraphQL;

namespace PlaywrightTests;

[TestClass]
public class SampleTests : PageTest
{
    [TestMethod]
    public async Task WopeeExample()
    {
        await Page.GotoAsync("https://playwright.dev");
        await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));

        var screenshot = await Page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
        var base64Screenshot = Convert.ToBase64String(screenshot);

        var scenarioName = $"Scenario_{TestContext.FullyQualifiedTestClassName}";
        var suiteUuid = WopeeTestHooks.SuiteUuid ?? throw new ArgumentNullException(nameof(WopeeTestHooks.SuiteUuid));

        var scenarioUuid = await WopeeTestHooks.CreateScenarioAsync(
            suiteUuid,
            scenarioName
        );
        Assert.IsNotNull(scenarioUuid, "ScenarioUuid not set; check ClassInit.");

        var stepName = "Sample step name - banana";
        var imageBase64 = base64Screenshot;

        var stepId = await WopeeTestHooks.CreateStepAsync(
            stepName,
            scenarioUuid,
            imageBase64
        );
        Assert.IsNotNull(stepId, "Expected to get a stepId back from createStep mutation.");
    }

    [TestMethod]
    public async Task HasTitle()
    {
        await Page.GotoAsync("https://playwright.dev");
        await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));
    }

    [TestMethod]
    public async Task GetStartedLink()
    {
        await Page.GotoAsync("https://playwright.dev");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
    } 
}
