using OpenQA.Selenium;

namespace E2E.Tests.Pages;

public class SummaryPage
{
    IWebDriver _driver;

    private readonly By _rankCalculatedId = By.Id("RankCalculated");
    private readonly By _similarityCalculatedId = By.Id("SimilarityCalculated");
    private readonly string _similarityLabel = "Плагиат:";
    private readonly string _rankLabel = "Оценка содержания:";

    public SummaryPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public IWebElement GetRankText()
    {
        return _driver.FindElement(_rankCalculatedId);
    }

    public IWebElement GetSimilarityText()
    {
        return _driver.FindElement(_similarityCalculatedId);
    }

    public bool IsRankAndSimilarityEqualTo(string expectedRank, string expectedSimilarity)
    {
        string expectedTextRank = $"{_rankLabel} {expectedRank}";
        string expectedTextSimilarity = $"{_similarityLabel} {expectedSimilarity}";

        int maxAttempts = 5;
        bool resultsFound = false;
        string rankValue = "";
        string similarityValue = "";

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                IWebElement rankElement = GetRankText();
                IWebElement similarityElement = GetSimilarityText();

                rankValue = rankElement.Text;
                similarityValue = similarityElement.Text;

                if (!string.IsNullOrEmpty(rankValue) && !string.IsNullOrEmpty(similarityValue) &&
                    rankValue.Contains(_rankLabel) && similarityValue.Contains(_similarityLabel))
                {
                    resultsFound = true;
                    break;
                }

                _driver.Navigate().Refresh();
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"Attempt {attempt + 1}: Elements not found yet");
            }

            Thread.Sleep(3000);
        }

        if (!resultsFound)
        {
            throw new Exception("Failed to get results after " + maxAttempts + " attempts");
        }

        return expectedTextSimilarity == similarityValue && expectedTextRank == rankValue;
    }
}
