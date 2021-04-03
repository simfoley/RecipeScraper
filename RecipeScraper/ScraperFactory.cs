﻿using RecipeScraper.Scrapers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeScraper
{
    public static class ScraperFactory
    {
        public static Dictionary<String, Func<String, BaseScraper>> scrapers;

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

        static ScraperFactory()
        {
            scrapers = new Dictionary<string, Func<string, BaseScraper>>()
            {
                {"troisfoisparjour.com", (x)=>new TroisFoisParJourScraper(x)},
                { "lecoupdegrace.ca",  (x)=>new LeCoupDeGrace(x)}
            };

        }
    }
}
