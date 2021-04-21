using System;
using System.Collections.Generic;
using System.Text;
using RecipeScraper;
using RecipeScraper.Scrapers;
using Xunit;

namespace RecipeScraper.IntegrationTests
{
    public class ScraperFactoryTests
    {
        [Fact]
        public void GetScraper_InvalidUri_ThrowsUriFormatException()
        {
            //Assert
            Assert.Throws<UriFormatException>(() => ScraperFactory.GetScraper("test"));
        }

        [Fact]
        public void GetScraper_UnsupportedWebpage_ThrowsNotSupportedException()
        {
            //Assert
            Assert.Throws<NotSupportedException>(() => ScraperFactory.GetScraper("https://www.unsupportedsite.com/recipe99"));
        }

        [Fact]
        public void GetScraper_SchemaOrgCompliantWebpage_ReturnsBaseScraper()
        {
            //Act
            var scraper = ScraperFactory.GetScraper("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best");

            //Assert
            Assert.IsType<BaseScraper>(scraper);
        }

        [Fact]
        public void GetScraper_CustomScraperHandledWebpage_ReturnsCustomScraper()
        {
            //Act
            var scraper = ScraperFactory.GetScraper("https://lecoupdegrace.ca/en/recette/grilled-spatchcock-turkey-with-dry-maple/");

            //Assert
            Assert.IsType<LeCoupDeGraceScraper>(scraper);
        }

        [Fact]
        public void AddCustomScraperAndGetScraper_CustomScraperHandledWebpage_ReturnsAddedCustomScraper()
        {
            //Act
            ScraperFactory.AddCustomScraper("custom-scraper-webpage.com", (x) => new TestCustomScraper(x));
            var scraper = ScraperFactory.GetScraper("https://custom-scraper-webpage.com/recipe99");

            //Assert
            Assert.IsType<TestCustomScraper>(scraper);
        }
    }

    public class TestCustomScraper : BaseScraper
    {
        public TestCustomScraper(string url) : base(url)
        {
        }
    }
}
