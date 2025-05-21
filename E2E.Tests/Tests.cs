using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace System.E2ETests;

public class ValuatorE2ETests : IDisposable
{
    private readonly IWebDriver _driver;
    private const string BaseUrl = "http://localhost:5001";


    private readonly string _rankCalculatedId = "RankCalculated";
    private readonly string _similarityCalculatedId = "SimilarityCalculated";
    private readonly string _similarityText = "a1b2";
    private readonly string _similarityLabel = "Плагиат:";
    private readonly string _rankLabel = "Оценка содержания:";

    public ValuatorE2ETests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        
        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    [Fact]
    public void SubmitTextAndCheckRank_FirstSending_ShouldReturnResults()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        Thread.Sleep(2000);

        var regionSelect = _driver.FindElement(By.Name("region"));
        var selectElement = new SelectElement(regionSelect);
        selectElement.SelectByValue("Russia");

        var textArea = _driver.FindElement(By.Id("field"));
        var submitButton = _driver.FindElement(By.Id("submit"));

        var testText = _similarityText;
        var expectedRank = $"{_rankLabel} 0,5";
        var expectedSimilarity = $"{_similarityLabel} 0";

        textArea.Clear();
        textArea.SendKeys(testText);
        submitButton.Click();

        Thread.Sleep(5000);

        var rankElement = _driver.FindElement(By.Id(_rankCalculatedId));
        var similarityElement = _driver.FindElement(By.Id(_similarityCalculatedId));

        string rankValue = rankElement.Text;
        string similarityValue = similarityElement.Text;

        Assert.That(expectedRank, Is.EqualTo(rankValue));
        Assert.That(expectedSimilarity, Is.EqualTo(similarityValue));
    }

    [Fact]
    public void SubmitTextAndCheckRank_SecondSending_ShouldReturnResults()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        Thread.Sleep(2000);

        var regionSelect = _driver.FindElement(By.Name("region"));
        var selectElement = new SelectElement(regionSelect);
        selectElement.SelectByValue("Russia");

        var textArea = _driver.FindElement(By.Id("field"));
        var submitButton = _driver.FindElement(By.Id("submit"));

        var testText = _similarityText;
        var expectedRank = $"{_rankLabel} 0,5";
        var expectedSimilarity = $"{_similarityLabel} 1";

        textArea.Clear();
        textArea.SendKeys(testText);
        submitButton.Click();

        Thread.Sleep(5000);

        var rankElement = _driver.FindElement(By.Id(_rankCalculatedId));
        var similarityElement = _driver.FindElement(By.Id(_similarityCalculatedId));

        string rankValue = rankElement.Text;
        string similarityValue = similarityElement.Text;

        Assert.That(expectedRank, Is.EqualTo(rankValue));
        Assert.That(expectedSimilarity, Is.EqualTo(similarityValue));
    }

    [Fact]
    public void SubmitTextAndCheckRank_ThirdSendingAnotherRegion_ShouldReturnResultsSimilarityFalse()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        Thread.Sleep(2000);

        var regionSelect = _driver.FindElement(By.Name("region"));
        var selectElement = new SelectElement(regionSelect);
        selectElement.SelectByValue("Germany");

        var textArea = _driver.FindElement(By.Id("field"));
        var submitButton = _driver.FindElement(By.Id("submit"));

        var testText = _similarityText;
        var expectedRank = $"{_rankLabel} 0,5";
        var expectedSimilarity = $"{_similarityLabel} 0";

        textArea.Clear();
        textArea.SendKeys(testText);
        submitButton.Click();

        Thread.Sleep(5000);

        var rankElement = _driver.FindElement(By.Id(_rankCalculatedId));
        var similarityElement = _driver.FindElement(By.Id(_similarityCalculatedId));

        string rankValue = rankElement.Text;
        string similarityValue = similarityElement.Text;

        Assert.That(expectedRank, Is.EqualTo(rankValue));
        Assert.That(expectedSimilarity, Is.EqualTo(similarityValue));
    }

    [TearDown]
    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}