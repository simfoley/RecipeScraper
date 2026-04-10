using RecipeScraper.Scrapers;
using System;
using System.Collections.Generic;

namespace RecipeScraper.Models;

public class RecipeScraperOptions
{
    internal Dictionary<string, Func<RecipeScraperBase>> CustomScrapers { get; } = new();

    public RecipeScraperOptions AddCustomScraper<TScraper>(string hostname)
        where TScraper : RecipeScraperBase, new()
    {
        CustomScrapers[hostname] = () => new TScraper();
        return this;
    }
}
