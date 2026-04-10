using RecipeScraper.Factory;
using RecipeScraper.Models;
using RecipeScraper.Service;
using System;
using Xunit;

namespace RecipeScraper.IntegrationTests.Scrapers
{
    public class ScraperFixture
    {
        public ScrapedRecipe JsonLdRecipe { get; }
        public ScrapedRecipe MicrodataRecipe { get; }

        public ScraperFixture()
        {
            var service = new RecipeScraperService(new ScraperFactory());
            JsonLdRecipe = service.ScrapeRecipe("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best").GetAwaiter().GetResult();
            MicrodataRecipe = service.ScrapeRecipe("https://www.metro.ca/en/recipes-occasions/recipes/vegetable-stew-with-vegetarian-balls").GetAwaiter().GetResult();
        }
    }

    public class BaseScraperTests : IClassFixture<ScraperFixture>
    {
        private readonly ScrapedRecipe _jsonLdRecipe;
        private readonly ScrapedRecipe _microdataRecipe;

        public BaseScraperTests(ScraperFixture fixture)
        {
            _jsonLdRecipe = fixture.JsonLdRecipe;
            _microdataRecipe = fixture.MicrodataRecipe;
        }

        // Json-LD
        [Fact]
        public void JsonLd_Name_IsReturned()
        {
            Assert.Equal("Barbecue Chicken Skewers (The Best)", _jsonLdRecipe.Name);
        }

        [Fact]
        public void JsonLd_Yield_IsReturned()
        {
            Assert.Equal("4 serving(s)", _jsonLdRecipe.Yield);
        }

        [Fact]
        public void JsonLd_PrepTime_IsReturned()
        {
            Assert.Equal(TimeSpan.FromMinutes(20), _jsonLdRecipe.PrepTime);
        }

        [Fact]
        public void JsonLd_CookTime_IsReturned()
        {
            Assert.Equal(TimeSpan.FromMinutes(15), _jsonLdRecipe.CookTime);
        }

        [Fact]
        public void JsonLd_Image_IsReturned()
        {
            Assert.Equal("https://images.ricardocuisine.com/services/recipes/1x1/9043.jpg", _jsonLdRecipe.Image);
        }

        [Fact]
        public void JsonLd_Ingredients_AreReturned()
        {
            Assert.Equal(15, _jsonLdRecipe.RecipeIngredients.Length);
            Assert.Equal("\u00bd cup (125 ml) buttermilk", _jsonLdRecipe.RecipeIngredients[0]);
        }

        [Fact]
        public void JsonLd_Instructions_AreReturned()
        {
            Assert.Equal(6, _jsonLdRecipe.RecipeInstructions.Length);
            Assert.Equal("In a glass dish, combine the buttermilk and spices. Add the chicken. Season with salt and pepper. Cover and refrigerate for 24 hours.", _jsonLdRecipe.RecipeInstructions[0]);
        }

        // Microdata
        [Fact]
        public void Microdata_Name_IsReturned()
        {
            Assert.Equal("Vegetable Stew with Vegetarian Balls", _microdataRecipe.Name);
        }

        [Fact]
        public void Microdata_Yield_IsReturned()
        {
            Assert.Equal("4", _microdataRecipe.Yield);
        }

        [Fact]
        public void Microdata_PrepTime_IsReturned()
        {
            Assert.Equal(TimeSpan.FromMinutes(30), _microdataRecipe.PrepTime);
        }

        [Fact]
        public void Microdata_CookTime_IsReturned()
        {
            Assert.Equal(TimeSpan.FromMinutes(60), _microdataRecipe.CookTime);
        }

        [Fact]
        public void Microdata_Image_IsReturned()
        {
            Assert.Equal("https://www.metro.ca/userfiles/image/recipes/Ragout-legumes-avec-boulettes-vegetariennes-4935.jpg", _microdataRecipe.Image);
        }

        [Fact]
        public void Microdata_Ingredients_AreReturned()
        {
            Assert.Equal(18, _microdataRecipe.RecipeIngredients.Length);
            Assert.Equal("3 carrots, cut into rounds", _microdataRecipe.RecipeIngredients[1]);
        }

        [Fact]
        public void Microdata_Instructions_AreReturned()
        {
            Assert.Equal(3, _microdataRecipe.RecipeInstructions.Length);
            Assert.Equal("In a saucepan, simmer the vegetables for 30 minute in the water and soya sauce.", _microdataRecipe.RecipeInstructions[0]);
        }
    }
}
