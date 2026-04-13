using Microsoft.Extensions.DependencyInjection;
using RecipeScraper.Extensions;
using RecipeScraper.Service.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace RecipeScraper.IntegrationTests;

/// <summary>
/// Verifies the full DI stack: AddRecipeScraper() wires up IRecipeScraperService
/// correctly and the service is able to scrape a recipe end-to-end.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddRecipeScraper_WiresServiceCorrectly_ReturnsRecipe()
    {
        var services = new ServiceCollection();
        services.AddRecipeScraper();
        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IRecipeScraperService>();
        var recipe = await service.ScrapeRecipe("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best");

        Assert.Equal("Barbecue Chicken Skewers (The Best)", recipe.Name);
    }

    [Fact]
    public async Task AddRecipeScraper_WithCustomScraper_UsesCustomScraper()
    {
        var services = new ServiceCollection();
        services.AddRecipeScraper(options =>
            options.AddCustomScraper<CustomTestScraper>("ricardocuisine.com"));
        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IRecipeScraperService>();
        var recipe = await service.ScrapeRecipe("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best");

        // CustomTestScraper overrides GetName to return a fixed value,
        // confirming the factory routed to the registered custom scraper
        Assert.Equal("custom", recipe.Name);
    }
}

public class CustomTestScraper : RecipeScraper.Scrapers.RecipeScraperBase
{
    public override string? GetName() => "custom";
}
