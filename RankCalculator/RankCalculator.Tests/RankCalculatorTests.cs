using Valuator;

namespace RankCalculator.Tests;

public class RankCalculatorTests
{
    [Fact]
    public void CalculateRank_EmptyText_ReturnsRankZero()
    {
        //Arrange
        string text = "";
        double expectedValue = 0;

        //Act
        double result = RankCalculator.CalculateRank(text);

        //Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void CalculateRank_WithOnlySpaces_ReturnsRankOne()
    {
        //Arrange
        string text = "   ";
        double expectedValue = 1.0;

        //Act
        double result = RankCalculator.CalculateRank(text);

        //Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void CalculateRank_WithTwoNewLinesSymbols_CountsNewLinesNonLetters()
    {
        //Arrange
        string text = "a\nb\nc";
        double expectedValue = 2.0/5.0;

        //Act
        double result = RankCalculator.CalculateRank(text);

        //Assert
        Assert.Equal(expectedValue, result);
    }


    //добавил корректную обработку эмодзи
    [Theory]
    [InlineData("abc", 0)]
    [InlineData("ABC", 0)]
    [InlineData("123", 1.0)]
    [InlineData("!@#", 1.0)]
    [InlineData("a1b2", 0.5)]
    [InlineData("t e s t", 3.0 / 7.0)]
    [InlineData("Тест", 0)]
    [InlineData("10000000000000000000000000000000000000000000000000", 1.0)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 0)]
    [InlineData("Emoji👍", 1.0 / 6.0)]
    [InlineData("AnotherEmoji😀", 1.0 / 13.0)]
    [InlineData("你好", 0)]
    public void CalculateRank_DifferentTestsInputsWith_ReturnsRankZero(string text, double expectedValue)
    {
        //Act
        double result = RankCalculator.CalculateRank(text);

        //Assert
        Assert.Equal(expectedValue, result);
    }
}