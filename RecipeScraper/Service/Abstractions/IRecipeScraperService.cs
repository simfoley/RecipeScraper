using RecipeScraper.Models;
using System.Threading.Tasks;

namespace RecipeScraper.Service;

public interface IRecipeScraperService
{
    Task<ScrapedRecipe> ScrapeRecipe(string url);
}
