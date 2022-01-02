using RecipeScraperLib.Scrapers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("RecipeScraper.IntegrationTests")]
namespace RecipeScraperLib.Factory
{
    internal static class ScraperFactory
    {
        private static Dictionary<String, Func<String, BaseScraper>> scrapers;

        public static BaseScraper GetScraper(string url)
        {
            string hostname = new Uri(url).Host.Replace("www.", "");

            if (!scrapers.ContainsKey(hostname)) 
            {
                //Verify that the webpage page contains a schema.org recipe object to scrap
                var scraper = new BaseScraper(url);

                if (!scraper.PageContentIsValid())
                {
                    throw new NotSupportedException("This webpage does not contain a schema.org recipe object to scrap.");
                }

                return scraper;
            }

            Func<String, BaseScraper> create = scrapers[hostname];

            return create(url);
        }
        
        public static void AddCustomScraper(string hostname, Func<string, BaseScraper> customScraper)
        {
            scrapers.Add(hostname, customScraper);
        }

        static ScraperFactory()
        {
            scrapers = new Dictionary<string, Func<string, BaseScraper>>()
            {
                { "lecoupdegrace.ca",  (x) => new LeCoupDeGraceScraper(x)}
            };
        }
    }
}
