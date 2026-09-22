using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BlazorApp.E2E;

public sealed class AskSurveyTests : IClassFixture<BlazorAppFixture>
{
    private readonly BlazorAppFixture fixture;

    public AskSurveyTests(BlazorAppFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void UserCanCompleteTheSurvey()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl($"{fixture.BaseUrl}/Ask");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(element => ((IJavaScriptExecutor)element).ExecuteScript(
            "return document.readyState === 'complete' && typeof Blazor !== 'undefined';"));
        wait.Until(element =>
        {
            var panel = element.FindElements(By.CssSelector(".ask-panel"));
            if (panel.Count > 0)
            {
                return true;
            }

            var toggle = element.FindElement(By.CssSelector("button[aria-label='Open survey']"));
            ((IJavaScriptExecutor)element).ExecuteScript("arguments[0].click();", toggle);
            return false;
        });

        Assert.Contains("Czy znalazłeś(-łaś) informacje", driver.PageSource);

        driver.FindElement(By.CssSelector("button[aria-label='4 star']")).Click();
        wait.Until(element => element.FindElement(By.CssSelector("button.ask-next"))).Click();

        Assert.Contains("Jak oceniasz jakość tej strony?", driver.PageSource);
        driver.FindElement(By.CssSelector("button[aria-label='5 star']")).Click();
        wait.Until(element => element.FindElement(By.CssSelector("button.ask-next"))).Click();

        Assert.Contains("Dziękujemy za Twoją opinię!", driver.PageSource);
        driver.FindElement(By.CssSelector("button.ask-close-survey")).Click();
        wait.Until(element => element.FindElement(By.CssSelector("button[aria-label='Open survey']")));
        Assert.DoesNotContain("Dziękujemy za Twoją opinię!", driver.PageSource);
    }

    private static IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        if (Environment.GetEnvironmentVariable("SELENIUM_BROWSER") == "chrome-visible")
        {
            options = new ChromeOptions();
            options.AddArgument("--window-size=1280,900");
        }

        return new ChromeDriver(options);
    }
}
