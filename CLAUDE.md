# RecipeScraper — Codebase Guide

## Project structure

```
RecipeScraper/                        # Main library
  Extensions/
    ServiceCollectionExtensions.cs    # AddRecipeScraper() DI extension
  Factory/
    Abstractions/IScraperFactory.cs   # GetScraper(url) interface
    ScraperFactory.cs                 # Hostname-based scraper lookup
  Models/
    RecipeScraperOptions.cs           # Options / custom scraper registration
    ScrapedRecipe.cs                  # Return model
  Parsers/
    Abstractions/IDocumentParser.cs   # Interface for all parsers
    JsonLdParser.cs                   # Parses schema.org JSON-LD scripts
    MicrodataParser.cs                # Parses schema.org Microdata attributes
    HtmlTreeParser.cs                 # Heuristic HTML tree fallback
  Scrapers/
    Abstractions/IRecipeScraper.cs    # ScrapeRecipe(url) interface
    RecipeScraperBase.cs              # Base class: fetches page, runs parsers
    LeCoupDeGraceScraper.cs           # Site-specific scraper example
  Service/
    Abstractions/IRecipeScraperService.cs
    RecipeScraperService.cs           # Public entry point via DI
  Helpers/
    AngleSharpHelpers.cs              # DOM traversal utilities

RecipeScraper.IntegrationTests/       # xUnit integration tests (real HTTP)
RecipeScraper.UnitTests/              # xUnit unit tests (no IO)
```

## How it works

1. A consumer calls `IRecipeScraperService.ScrapeRecipe(url)`
2. `RecipeScraperService` delegates to `IScraperFactory.GetScraper(url)` — a pure hostname lookup that returns the appropriate `IRecipeScraper`
3. `IRecipeScraper.ScrapeRecipe(url)` (implemented by `RecipeScraperBase`) fetches the page via AngleSharp and initialises three parsers in priority order: `JsonLdParser` → `MicrodataParser` → `HtmlTreeParser`
4. Each `Get*` method iterates the parsers and returns the first non-null result

## ScrapedRecipe model

| Property                | Type        | Description                        |
| ----------------------- | ----------- | ---------------------------------- |
| `Name`                  | `string?`   | Recipe name                        |
| `Image`                 | `string?`   | Image URL                          |
| `Yield`                 | `string?`   | Serving size / yield               |
| `PrepTime`              | `TimeSpan?` | Preparation time                   |
| `CookTime`              | `TimeSpan?` | Cook time                          |
| `TotalTime`             | `TimeSpan?` | Total time                         |
| `RecipeIngredients`     | `string[]`  | List of ingredients                |
| `RecipeInstructions`    | `string[]`  | List of instruction steps          |
| `RecipeLanguageISOCode` | `string?`   | Language of the page (e.g. `"en"`) |

## Entry point for consumers

```csharp
// Registration
builder.Services.AddRecipeScraper(options =>
{
    options.AddCustomScraper<MySiteScraper>("mysite.com");
});

// Usage
var recipe = await scraper.ScrapeRecipe(url); // IRecipeScraperService
```

## Adding a custom scraper

Inherit `RecipeScraperBase` and override any `Get*` virtual methods. `_pageContent` is an AngleSharp `IDocument` available after `ScrapeRecipe` is called.

```csharp
public class MySiteScraper : RecipeScraperBase
{
    public override string[] GetRecipeIngredients() { ... }
}
```

Register it with `options.AddCustomScraper<MySiteScraper>("hostname.com")`.

## Build & test

```bash
dotnet build
dotnet test
```

Integration tests make real HTTP requests. Unit tests in `RecipeScraper.UnitTests` are pure (no IO).

## Key namespaces

| Namespace | Contents |
|---|---|
| `RecipeScraper.Service` | `IRecipeScraperService`, `RecipeScraperService` |
| `RecipeScraper.Models` | `ScrapedRecipe`, `RecipeScraperOptions` |
| `RecipeScraper.Scrapers` | `RecipeScraperBase`, site-specific scrapers |
| `RecipeScraper.Parsers` | `JsonLdParser`, `MicrodataParser`, `HtmlTreeParser` |
| `RecipeScraper.Factory` | `IScraperFactory`, `ScraperFactory` |
| `RecipeScraper.Extensions` | `AddRecipeScraper()` |
