# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ScraperFactoryTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~BaseScraper_GetNameFromJsonLdWebpage_NameIsReturned"

# Pack NuGet package
dotnet pack --configuration Release RecipeScraper/RecipeScraper.csproj --output . /p:Version=X.X.X
```

## Architecture

RecipeScraper is a .NET 10 class library (distributed as NuGet) that extracts structured recipe data from web pages. Entry point is the static `RecipeScraper.ScrapeRecipe(url)` method.

### Scraping Pipeline

```
RecipeScraper.ScrapeRecipe(url)
  → ScraperFactory.GetScraper(url)         # hostname lookup → custom scraper or BaseScraper
  → BaseScraper fetches page via AngleSharp
  → For each field, tries format scrapers in order:
      1. JsonLdScraper      (schema.org JSON-LD in <script> tags)
      2. MicrodataScraper   (HTML5 itemprop/itemtype attributes)
      3. HtmlTreeParsingScraper  (heuristic/keyword fallback)
  → Returns ScrappedRecipe DTO
```

**Format scrapers** (`IFormatScraper`) are tried in priority order. The first one that returns a non-empty result wins. Each recipe field is extracted independently — a failure on one field does not affect others (wrapped with `IgnoreExceptions()`).

### Extending the Library

**Add support for a new website** — two options:
1. If the site uses standard schema.org markup, `BaseScraper` handles it automatically.
2. If the site needs custom parsing, subclass `BaseScraper`, override virtual methods (`GetName()`, `GetRecipeIngredients()`, etc.), and register in `ScraperFactory`'s static constructor with the hostname key.

See `LeCoupDeGraceScraper` as the reference example for a custom scraper.

### Key Namespaces

- `RecipeScraperLib` — public API (`RecipeScraper`, `ScrappedRecipe`)
- `RecipeScraperLib.Scrapers` — `BaseScraper` and site-specific scrapers
- `RecipeScraper.Scrapers.FormatScrapers` — `IFormatScraper` + implementations (internal)
- `RecipeScraperLib.Factory` — `ScraperFactory` (internal)
- `RecipeScraper.Helpers` — `AngleSharpHelpers` static utilities (internal)

### Tests

Integration tests in `RecipeScraper.IntegrationTests/` hit real URLs (ricardocuisine.com for JSON-LD, metro.ca for microdata). Tests run in parallel (`xunit.runner.json`). Test naming convention: `ClassName_MethodName_ExpectedBehavior`.

CI runs on push/PR to master (`.github/workflows/master.yml`). Releases are triggered by semver tags and publish to NuGet (`.github/workflows/release.yml`).
