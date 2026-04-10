using RecipeScraper.Models;
using System.Threading.Tasks;

namespace RecipeScraper.Service.Abstractions;

public interface IRecipeScraperService
{
    Task<ScrapedRecipe> ScrapeRecipe(string url);
}
