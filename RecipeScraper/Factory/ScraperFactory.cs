using RecipeScraper.Factory.Abstractions;
using RecipeScraper.Models;
using RecipeScraper.Scrapers;
using RecipeScraper.Scrapers.Abstractions;
using System;
using System.Collections.Generic;

namespace RecipeScraper.Factory;

public class ScraperFactory : IScraperFactory
{
    private readonly Dictionary<string, Func<RecipeScraperBase>> _scrapers;

    public ScraperFactory() : this(new RecipeScraperOptions()) { }

    public ScraperFactory(RecipeScraperOptions options)
    {
        _scrapers = new Dictionary<string, Func<RecipeScraperBase>>
        {
            { "lecoupdegrace.ca", () => new LeCoupDeGraceScraper() }
        };

        foreach (var (hostname, factory) in options.CustomScrapers)
            _scrapers[hostname] = factory;
    }

    public IRecipeScraper GetScraper(string url)
    {
        string hostname = new Uri(url).Host.Replace("www.", "");

        if (_scrapers.TryGetValue(hostname, out var factory))
            return factory();

        return new RecipeScraperBase();
    }
}
