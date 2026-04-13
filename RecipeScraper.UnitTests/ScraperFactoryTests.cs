using RecipeScraper.Factory;
using RecipeScraper.Factory.Abstractions;
using RecipeScraper.Models;
using RecipeScraper.Scrapers;
using System;
using Xunit;

namespace RecipeScraper.UnitTests;

public class ScraperFactoryTests
{
    private readonly IScraperFactory _factory = new ScraperFactory();

    [Fact]
    public void GetScraper_InvalidUri_ThrowsUriFormatException()
    {
        Assert.Throws<UriFormatException>(() => _factory.GetScraper("not-a-url"));
    }

    [Fact]
    public void GetScraper_UnregisteredHostname_ReturnsRecipeScraperBase()
    {
        var scraper = _factory.GetScraper("https://unknown-site.com/recipe/123");
        Assert.IsType<RecipeScraperBase>(scraper);
    }

    [Fact]
    public void GetScraper_LeCoupDeGraceHostname_ReturnsLeCoupDeGraceScraper()
    {
        var scraper = _factory.GetScraper("https://lecoupdegrace.ca/en/recette/example");
        Assert.IsType<LeCoupDeGraceScraper>(scraper);
    }

    [Fact]
    public void GetScraper_UrlWithWwwPrefix_StripsWwwAndReturnsCorrectScraper()
    {
        var scraper = _factory.GetScraper("https://www.lecoupdegrace.ca/en/recette/example");
        Assert.IsType<LeCoupDeGraceScraper>(scraper);
    }

    [Fact]
    public void GetScraper_CustomRegisteredHostname_ReturnsCustomScraper()
    {
        var options = new RecipeScraperOptions().AddCustomScraper<TestCustomScraper>("custom-site.com");
        var factory = new ScraperFactory(options);

        var scraper = factory.GetScraper("https://custom-site.com/recipe/123");

        Assert.IsType<TestCustomScraper>(scraper);
    }

    [Fact]
    public void GetScraper_CustomRegisteredHostnameWithWwwPrefix_ReturnsCustomScraper()
    {
        var options = new RecipeScraperOptions().AddCustomScraper<TestCustomScraper>("custom-site.com");
        var factory = new ScraperFactory(options);

        var scraper = factory.GetScraper("https://www.custom-site.com/recipe/123");

        Assert.IsType<TestCustomScraper>(scraper);
    }
}

public class TestCustomScraper : RecipeScraperBase { }
