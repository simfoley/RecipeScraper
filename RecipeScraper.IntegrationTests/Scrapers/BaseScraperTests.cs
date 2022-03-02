using RecipeScraperLib.Factory;
using RecipeScraperLib.Scrapers;
using System;
using Xunit;

namespace RecipeScraperLib.IntegrationTests.Scrapers
{
    public class BaseScraperTests
    {
        BaseScraper _jsonLdScraper = ScraperFactory.GetScraper("https://www.ricardocuisine.com/en/recipes/9043-barbecue-chicken-skewers-the-best");
        BaseScraper _microdataScraper = ScraperFactory.GetScraper("https://www.metro.ca/en/recipes-occasions/recipes/vegetable-stew-with-vegetarian-balls");

        //JsonLd
        [Fact]
        public void BaseScraper_GetNameFromJsonLdWebpage_NameIsReturned()
        {
            //Act
            var recipeName = _jsonLdScraper.GetName();

            //Assert
            Assert.Equal("Barbecue Chicken Skewers (The Best)", recipeName);
        }

        [Fact]
        public void BaseScraper_GetYieldFromJsonLdWebpage_YieldIsReturned()
        {
            //Act
            var recipeYield = _jsonLdScraper.GetYield();

            //Assert
            Assert.Equal("4 serving(s)", recipeYield);
        }

        [Fact]
        public void BaseScraper_GetPrepTimeFromJsonLdWebpage_PrepTimeIsReturned()
        {
            //Act
            var recipePrepTime = _jsonLdScraper.GetPrepTime();

            //Assert
            Assert.Equal(TimeSpan.FromMinutes(20), recipePrepTime);
        }

        [Fact]
        public void BaseScraper_GetCookTimeFromJsonLdWebpage_CookTimeIsReturned()
        {
            //Act
            var recipeCookTime = _jsonLdScraper.GetCookTime();

            //Assert
            Assert.Equal(TimeSpan.FromMinutes(15), recipeCookTime);
        }

        [Fact]
        public void BaseScraper_GetImageFromJsonLdWebpage_ImageIsReturned()
        {
            //Act
            var recipeImage = _jsonLdScraper.GetImage();

            //Assert
            Assert.Equal("https://images.ricardocuisine.com/services/recipes/1x1/9043.jpg", recipeImage);
        }

        [Fact]
        public void BaseScraper_GetIngredientsFromJsonLdWebpage_IngredientsAreReturned()
        {
            //Act
            var recipeIngredients = _jsonLdScraper.GetRecipeIngredients();

            //Assert
            Assert.Equal(15, recipeIngredients.Length);
            Assert.Equal("\u00bd cup (125 ml) buttermilk", recipeIngredients[0]);
        }

        [Fact]
        public void BaseScraper_GetInstructionsFromJsonLdWebpage_InstructionsAreReturned()
        {
            //Act
            var recipeInstructions = _jsonLdScraper.GetRecipeInstructions();

            //Assert
            Assert.Equal(6, recipeInstructions.Length);
            Assert.Equal("In a glass dish, combine the buttermilk and spices. Add the chicken. Season with salt and pepper. Cover and refrigerate for 24 hours.", recipeInstructions[0]);
        }

        //Microdata
        [Fact]
        public void BaseScraper_GetNameFromMicrodataWebpage_NameIsReturned()
        {
            //Act
            var recipeName = _microdataScraper.GetName();

            //Assert
            Assert.Equal("Vegetable Stew with Vegetarian Balls", recipeName);
        }

        [Fact]
        public void BaseScraper_GetYieldMicrodataWebpage_YieldIsReturned()
        {
            //Act
            var recipeYield = _microdataScraper.GetYield();

            //Assert
            Assert.Equal("4", recipeYield);
        }

        [Fact]
        public void BaseScraper_GetPrepTimeFromMicrodataWebpage_PrepTimeIsReturned()
        {
            //Act
            var recipePrepTime = _microdataScraper.GetPrepTime();

            //Assert
            Assert.Equal(TimeSpan.FromMinutes(30), recipePrepTime);
        }

        [Fact]
        public void BaseScraper_GetCookTimeFromMicrodataWebpage_CookTimeIsReturned()
        {
            //Act
            var recipeCookTime = _microdataScraper.GetCookTime();

            //Assert
            Assert.Equal(TimeSpan.FromMinutes(60), recipeCookTime);
        }

        [Fact]
        public void BaseScraper_GetImageFromMicrodataWebpage_ImageIsReturned()
        {
            //Act
            var recipeImage = _microdataScraper.GetImage();

            //Assert
            Assert.Equal("https://www.metro.ca/userfiles/image/recipes/Ragout-legumes-avec-boulettes-vegetariennes-4935.jpg", recipeImage);
        }

        [Fact]
        public void BaseScraper_GetIngredientsFromMicrodataWebpage_IngredientsAreReturned()
        {
            //Act
            var recipeIngredients = _microdataScraper.GetRecipeIngredients();

            //Assert
            Assert.Equal(18, recipeIngredients.Length);
            Assert.Equal("3 carrots, cut into rounds", recipeIngredients[1]);
        }

        [Fact]
        public void BaseScraper_GetInstructionsFromMicrodataWebpage_InstructionsAreReturned()
        {
            //Act
            var recipeInstructions = _microdataScraper.GetRecipeInstructions();

            //Assert
            Assert.Equal(3, recipeInstructions.Length);
            Assert.Equal("In a saucepan, simmer the vegetables for 30 minute in the water and soya sauce.", recipeInstructions[0]);
        }
    }
}
