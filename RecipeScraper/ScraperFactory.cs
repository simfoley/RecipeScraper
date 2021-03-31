using RecipeScraper.Scrapers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeScraper
{
    public static class ScraperFactory
    {
        public static BaseScraper GetScraper(string url)
        {
            switch (new Uri(url).Host)
            {
                //TODO add other scrapers
                default:
                    return new BaseScraper(url);
            }
        }
    }
}
