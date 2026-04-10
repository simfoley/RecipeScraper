using RecipeScraper.Scrapers.Abstractions;

namespace RecipeScraper.Factory.Abstractions;

public interface IScraperFactory
{
    IRecipeScraper GetScraper(string url);
}
