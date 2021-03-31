# RecipeScraper

RecipeParser is a .Net Standard library I created for my website Mealzer.com. The goal is to be able to give it a url of a webpage that contains a recipe and it returns an object.

## Usage

`using RecipeScraper;
var scraper = ScraperFactory.GetScraper(url);
var scrapedRecipe = scraper.ScrapeRecipe();
`
