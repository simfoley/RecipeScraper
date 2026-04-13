# RecipeScraper

[![NuGet version (RecipeScraper)](https://img.shields.io/nuget/v/RecipeScraper.svg?style=flat-square)](https://www.nuget.org/packages/RecipeScraper/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![build](https://github.com/simfoley/RecipeScraper/actions/workflows/release.yml/badge.svg)](https://github.com/simfoley/RecipeScraper/actions/workflows/release.yml)
[![Github All Releases](https://img.shields.io/nuget/dt/RecipeScraper)](https://github.com/simfoley/RecipeScraper/releases)

RecipeScraper is a .NET library that extracts recipe data from any web page. Pass it a URL and it returns a structured recipe object. It uses [AngleSharp](https://anglesharp.github.io/) to parse the page and supports schema.org Recipe data in JSON-LD, Microdata, and HTML tree formats.

### [Releases](https://github.com/simfoley/RecipeScraper/releases)

## Requirements

- .NET 10.0+

## Usage

Install the NuGet package:

```
dotnet add package RecipeScraper
```

Register the service in your DI container:

```csharp
using RecipeScraper.Extensions;

builder.Services.AddRecipeScraper();
```

Inject `IRecipeScraperService` and call `ScrapeRecipe`:

```csharp
using RecipeScraper.Service;

public class MyClass(IRecipeScraperService scraper)
{
    public async Task Run()
    {
        var recipe = await scraper.ScrapeRecipe("https://www.foodnetwork.com/recipes/food-network-kitchen/baked-pork-chop-3631185");
    }
}
```

### Return value

`ScrapeRecipe` returns a `ScrapedRecipe` object with the following properties:

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

If a field cannot be found, it defaults to `null` or an empty array.

## Custom Scrapers

If a site doesn't use schema.org markup, you can register a site-specific scraper by inheriting from `RecipeScraperBase` and overriding whichever methods you need:

```csharp
using RecipeScraper.Scrapers;

public class MySiteScraper : RecipeScraperBase
{
    public override string[] GetRecipeIngredients()
    {
        // custom HTML parsing using _pageContent (AngleSharp IDocument)
        return [];
    }
}
```

Register it via the options callback in `AddRecipeScraper`:

```csharp
builder.Services.AddRecipeScraper(options =>
{
    options.AddCustomScraper<MySiteScraper>("mysite.com");
});
```

## Contributing

Pull requests are welcome. The library tries three parsing strategies in priority order:

1. **JSON-LD** — `<script type="application/ld+json">` containing a `schema.org/Recipe` object
2. **Microdata** — inline `itemtype="http://schema.org/Recipe"` attributes
3. **HTML tree** — fallback heuristic HTML parsing

To add support for a new site, create a class that inherits `RecipeScraperBase`, override the methods that differ, and register it via `AddCustomScraper`. Integration tests live in `RecipeScraper.IntegrationTests` — to add coverage for a new site, add an entry to `RecipeTestData.json` with the URL and expected values.

## License

MIT
