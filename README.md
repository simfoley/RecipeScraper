# RecipeScraper

[![NuGet version (RecipeScraper)](https://img.shields.io/nuget/v/RecipeScraper.svg?style=flat-square)](https://www.nuget.org/packages/RecipeScraper/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![build](https://github.com/simfoley/RecipeScraper/actions/workflows/release.yml/badge.svg)](https://github.com/simfoley/RecipeScraper/actions/workflows/release.yml)
[![Github All Releases](https://img.shields.io/nuget/dt/RecipeScraper)](https://github.com/simfoley/RecipeScraper/releases)

RecipeScraper is a .Net Standard library I created for my website that uses Anglesharp parser library. The goal of this project was to be able to pass it a url of a webpage containing a recipe and it returns an object.

### [Releases](https://github.com/simfoley/RecipeScraper/releases)

## Usage

Install the RecipeScraper Nuget Package.

`dotnet add package RecipeScraper`

Use it in your code by calling the factory with the recipe page url.

```
using RecipeScraperLib;

var scrapedRecipe = RecipeScraper.ScrapeRecipe("https://www.foodnetwork.com/recipes/food-network-kitchen/baked-pork-chop-3631185");
```

## Contribution and Possible Improvements

Pull requests are welcome. If you find a website with recipe pages not supported by the library, you can add a new scraper.



