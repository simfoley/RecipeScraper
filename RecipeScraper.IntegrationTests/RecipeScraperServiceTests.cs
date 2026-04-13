using RecipeScraper.Factory;
using RecipeScraper.Models;
using RecipeScraper.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RecipeScraper.IntegrationTests;

public class RecipeScraperServiceTestsFixture
{
    public Dictionary<string, ScrapedRecipe> Cache { get; }

    public RecipeScraperServiceTestsFixture()
    {
        var service = new RecipeScraperService(new ScraperFactory());
        Cache = RecipeTestData.All
            .DistinctBy(tc => tc.Url)
            .ToDictionary(
                tc => tc.Url,
                tc => service.ScrapeRecipe(tc.Url).GetAwaiter().GetResult()
            );
    }
}

public class RecipeScraperServiceTests : IClassFixture<RecipeScraperServiceTestsFixture>
{
    private readonly RecipeScraperServiceTestsFixture _fixture;

    public RecipeScraperServiceTests(RecipeScraperServiceTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void Name_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.Name, scrapedRecipe.Name);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void Image_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.Image, scrapedRecipe.Image);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void Yield_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.Yield, scrapedRecipe.Yield);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void PrepTime_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.PrepTime, scrapedRecipe.PrepTime);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void CookTime_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.CookTime, scrapedRecipe.CookTime);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void TotalTime_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.TotalTime, scrapedRecipe.TotalTime);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void Ingredients_AreReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        AssertStringArrays(testCase.Ingredients, scrapedRecipe.RecipeIngredients);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void Instructions_AreReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        AssertStringArrays(testCase.Instructions, scrapedRecipe.RecipeInstructions);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData.Sites), MemberType = typeof(RecipeTestData))]
    public void LanguageCode_IsReturned(RecipeTestCase testCase)
    {
        var scrapedRecipe = _fixture.Cache[testCase.Url];
        Assert.Equal(testCase.LanguageCode, scrapedRecipe.RecipeLanguageISOCode);
    }



    private static void AssertStringArrays(string[] expected, string[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] == actual[i]) continue;
            int pos = Enumerable.Range(0, Math.Min(expected[i].Length, actual[i].Length))
                .FirstOrDefault(j => expected[i][j] != actual[i][j], Math.Min(expected[i].Length, actual[i].Length));
            throw new Xunit.Sdk.XunitException(
                $"AssertStringArrays - Difference at index [{i}], position [{pos}]\n" +
                $"Expected: \"{expected[i]}\"\n" +
                $"Actual:   \"{actual[i]}\"");
        }
    }
}
