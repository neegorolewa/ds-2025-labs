using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace E2E.Tests.Pages;

public class IndexPage
{
    private readonly IWebDriver _driver;

    private readonly By _regionSelect = By.Name("region");
    private readonly By _textArea = By.Id("field");
    private readonly By _submitButton = By.Id("submit");

    public IndexPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public IWebElement GetRegionSelecter()
    {
        return _driver.FindElement(_regionSelect);
    }

    public IWebElement GetTextArea()
    {
        return _driver.FindElement(_textArea);
    }

    public IWebElement GetSubmitButton()
    {
        return _driver.FindElement(_submitButton);
    }

    public void SelectRegion(string region)
    {
        IWebElement selector = GetRegionSelecter();
        var selectElement = new SelectElement(selector);

        selectElement.SelectByValue(region);
    }

    public void EnterTextToArea(string text)
    {
        IWebElement textArea = GetTextArea();
        
        textArea.Clear();
        textArea.SendKeys(text);
    }

    public void ClickSumbitButton()
    {
        IWebElement sumbitButton = GetSubmitButton();

        sumbitButton.Click();
    }
}
