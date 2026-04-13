using System;

namespace RecipeScraper.Models;

public class ScrapedRecipe
{
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? Yield { get; set; }
    public TimeSpan? PrepTime { get; set; }
    public TimeSpan? CookTime { get; set; }
    public TimeSpan? TotalTime { get; set; }

    public string[] RecipeIngredients { get; set; } = Array.Empty<string>();
    public string[] RecipeInstructions { get; set; } = Array.Empty<string>();
    public string? RecipeLanguageISOCode { get; set; }
}
