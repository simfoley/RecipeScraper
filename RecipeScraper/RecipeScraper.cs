using RecipeScraperLib.Factory;
using RecipeScraperLib.Models;
using RecipeScraperLib.Scrapers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeScraperLib
{
    public static class RecipeScraper
    {
        public static ScrappedRecipe ScrapeRecipe(string url)
        {
            var scraper = ScraperFactory.GetScraper(url);
            return scraper.ScrapeRecipe();
        }

        public static void AddCustomScraper(string hostname, Func<string, BaseScraper> customScraper)
        {
            ScraperFactory.AddCustomScraper(hostname, customScraper);
        }
    }
}
