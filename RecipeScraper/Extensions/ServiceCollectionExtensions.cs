using Microsoft.Extensions.DependencyInjection;
using RecipeScraper.Factory;
using RecipeScraper.Models;
using RecipeScraper.Service;
using System;

namespace RecipeScraper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRecipeScraper(
        this IServiceCollection services,
        Action<RecipeScraperOptions>? configure = null)
    {
        var options = new RecipeScraperOptions();
        configure?.Invoke(options);

        services.AddSingleton<IScraperFactory>(new ScraperFactory(options));
        services.AddSingleton<IRecipeScraperService, RecipeScraperService>();

        return services;
    }
}
