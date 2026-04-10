using RecipeScraper.Factory.Abstractions;
using RecipeScraper.Models;
using RecipeScraper.Service.Abstractions;
using System.Threading.Tasks;

namespace RecipeScraper.Service;

public class RecipeScraperService : IRecipeScraperService
{
    private readonly IScraperFactory _factory;

    public RecipeScraperService(IScraperFactory factory)
    {
        _factory = factory;
    }

    public async Task<ScrapedRecipe> ScrapeRecipe(string url)
    {
        return await _factory.GetScraper(url).ScrapeRecipe(url);
    }
}
