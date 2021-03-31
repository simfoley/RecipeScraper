# RecipeScraper

RecipeScraper is a .Net Standard library I created for my website [Mealzer.com](https://mealzer.com/) that uses Anglesharp. The goal of this project was to be able to pass it a url of a webpage containing a recipe and it returns an object.

## Usage

Install the RecipeScraper Nuget Package.

`dotnet add package RecipeScraper`

## Usage

```
using RecipeScraper;

var scraper = ScraperFactory.GetScraper(url);
var scrapedRecipe = scraper.ScrapeRecipe();
```

### [Releases](https://gitlab.com/simon.foley/RecipeScraper/-/tags)

## Contribution and Possible Improvements

Merge requests are welcome. If you find a website with recipe pages not supported by the library, you can add a new scraper.



