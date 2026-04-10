using RecipeScraper.Models;
using System.Threading.Tasks;

namespace RecipeScraper.Scrapers.Abstractions;

public interface IRecipeScraper
{
    Task<ScrapedRecipe> ScrapeRecipe(string url);
}
