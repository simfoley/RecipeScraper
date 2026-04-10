using RecipeScraper.Factory;
using RecipeScraper.Factory.Abstractions;
using RecipeScraper.Models;
using RecipeScraper.Scrapers;
using System;
using Xunit;

namespace RecipeScraper.UnitTests
{
    public class ScraperFactoryTests
    {
        private readonly IScraperFactory _factory = new ScraperFactory();

        [Fact]
        public void GetScraper_InvalidUri_ThrowsUriFormatException()
        {
            Assert.Throws<UriFormatException>(() => _factory.GetScraper("test"));
        }

        [Fact]
        public void GetScraper_UnregisteredHostname_ReturnsBaseScraper()
        {
            var scraper = _factory.GetScraper("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best");

            Assert.IsType<RecipeScraperBase>(scraper);
        }

        [Fact]
        public void GetScraper_RegisteredHostname_ReturnsCustomScraper()
        {
            var scraper = _factory.GetScraper("https://lecoupdegrace.ca/en/recette/grilled-spatchcock-turkey-with-dry-maple/");

            Assert.IsNotType<RecipeScraperBase>(scraper);
        }

        [Fact]
        public void GetScraper_CustomRegisteredHostname_ReturnsAddedScraper()
        {
            var options = new RecipeScraperOptions().AddCustomScraper<TestCustomScraper>("custom-scraper-webpage.com");
            var factory = new ScraperFactory(options);

            var scraper = factory.GetScraper("https://custom-scraper-webpage.com/recipe99");

            Assert.IsType<TestCustomScraper>(scraper);
        }
    }

    public class TestCustomScraper : RecipeScraperBase { }
}
