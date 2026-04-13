using System;

namespace RecipeScraper.IntegrationTests;

public record RecipeTestCase
{
    public string Url { get; init; } = "";
    public string? Name { get; init; }
    public string? Image { get; init; }
    public string? Yield { get; init; }
    public TimeSpan? PrepTime { get; init; }
    public TimeSpan? CookTime { get; init; }
    public TimeSpan? TotalTime { get; init; }
    public string[] Ingredients { get; init; } = Array.Empty<string>();
    public string[] Instructions { get; init; } = Array.Empty<string>();
    public string? LanguageCode { get; init; }

    public override string ToString() => new Uri(Url).Host;
}
