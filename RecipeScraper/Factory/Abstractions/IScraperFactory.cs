using RecipeScraper.Scrapers.Abstractions;

namespace RecipeScraper.Factory;

public interface IScraperFactory
{
    IRecipeScraper GetScraper(string url);
}
