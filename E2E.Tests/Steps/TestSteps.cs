using E2E.Tests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using Reqnroll.Plugins;

namespace E2E.Tests.Steps;

[Binding]
public class TestSteps : IDisposable
{
    IWebDriver? _driver;
    IndexPage? _indexPage;
    SummaryPage? _summaryPage;

    const string BaseUrl = "http://localhost:8080";

    [Given(@"I open web application")]
    public void OpenBrowser()
    {
        _driver = new ChromeDriver();

        _driver.Navigate().GoToUrl(BaseUrl);
        _indexPage = new(_driver);
        _summaryPage = new(_driver);

        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    [Given(@"I go to main page")]
    public void GoToMainPage()
    {
        _driver?.Navigate().GoToUrl(BaseUrl);
    }

    [When(@"I select region ""(.*)""")]
    public void WhenISelectRegion(string region)
    {
        _indexPage?.SelectRegion(region);
        Thread.Sleep(2000);
    }

    [When(@"I enter text ""(.*)""")]
    public void WhenIEnterText(string text)
    {
        _indexPage?.EnterTextToArea(text);
        Thread.Sleep(2000);
    }

    [When(@"I click the ""Submit"" button")]
    public void WhenClickTheSubmitButton()
    {
        _indexPage?.ClickSumbitButton();
        Thread.Sleep(1000);
    }

    [Then(@"I should see rank ""(.*)"" and similarity ""(.*)""")]
    public void CheckResult(string expectedRank, string expectedSimilarity)
    {
        Assert.True(_summaryPage?.IsRankAndSimilarityEqualTo(expectedRank, expectedSimilarity));
        Thread.Sleep(2000);
    }

    [AfterScenario]
    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
