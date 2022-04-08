using RecipeScraperLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Text.Json;
using System.Xml;
using System.Text.RegularExpressions;
using RecipeScraper.Scrapers.FormatScrapers;
using RecipeScraper.Scrapers.FromatScrapers;

namespace RecipeScraperLib.Scrapers
{
    //The BaseScraper should be able to handle pages compliant with http://schema.org/Recipe object, either with Json LD or with Microdata or both
    public class BaseScraper
    {
        protected string _url;
        protected IDocument _pageContent;
        private List<IFormatScraper> formatScrapers = new List<IFormatScraper>();

        public BaseScraper(string url)
        {
            _url = url;
            
            //Get page content
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            _pageContent = context.OpenAsync(url).Result;

            //Init FormatScrapers
            formatScrapers.Add(new JsonLdScraper(_pageContent));
            formatScrapers.Add(new MircrodataScraper(_pageContent));
            formatScrapers.Add(new HtlmTreeParsingScraper(_pageContent));
        }

        public bool PageContentIsValid()
        {
            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                    return true;
            }

            return false;
        }

        public ScrappedRecipe ScrapeRecipe()
        {
            return new ScrappedRecipe() 
            { 
                Name = IgnoreExceptions(() => GetName(), null),
                Image = IgnoreExceptions(() => GetImage(), null),
                Yield = IgnoreExceptions(() => GetYield(), null),
                PrepTime = IgnoreExceptions(() => GetPrepTime(), new TimeSpan()),
                CookTime = IgnoreExceptions(() => GetCookTime(), new TimeSpan()),
                RecipeIngredients = IgnoreExceptions(() => GetRecipeIngredients(), new string[0]),
                RecipeInstructions = IgnoreExceptions(() => GetRecipeInstructions(), new string[0]),
                RecipeLanguageISOCode = IgnoreExceptions(() => GetRecipeLanguageISOCode(), "en")
            };
        }

        //Method to ignore exceptions so we can keep trying to get other values from the webpage
        private T IgnoreExceptions<T>(Func<T> operation, T defaultValue)
        {
            if (operation == null)
                return defaultValue;

            T result;
            try
            {
                result = operation.Invoke();
            }
            catch
            {
                result = defaultValue;
            }

            return result;
        }

        public virtual string GetName()
        {
            string name = null;

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    name = formatScraper.GetName();
                    if (!string.IsNullOrEmpty(name))
                        break;
                }
            }

            return name;
        }

        public virtual string GetImage()
        {
            string imageSource = null;

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    imageSource = formatScraper.GetImage();
                    if (!string.IsNullOrEmpty(imageSource))
                        break;
                }
            }

            return imageSource;
        }

        public virtual string GetYield()
        {
            string yield = null;

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    yield = formatScraper.GetYield();
                    if (!string.IsNullOrEmpty(yield))
                        break;
                }
            }

            return yield;
        }

        public virtual TimeSpan GetPrepTime()
        {
            string prepTime = string.Empty;

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    prepTime = formatScraper.GetPrepTime();
                    if (!string.IsNullOrEmpty(prepTime))
                        break;
                }
            }

            return XmlConvert.ToTimeSpan(prepTime);
        }

        public virtual TimeSpan GetCookTime()
        {
            string cookTime = string.Empty;

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    cookTime = formatScraper.GetCookTime();
                    if (!string.IsNullOrEmpty(cookTime))
                        break;
                }
            }

            return XmlConvert.ToTimeSpan(cookTime);
        }

        public virtual string[] GetRecipeIngredients() 
        {
            var recipeIngredients = new List<string>();

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    recipeIngredients = formatScraper.GetRecipeIngredients();
                    if (recipeIngredients.Count > 0)
                        break;
                }
            }

            return recipeIngredients.ToArray();
        }

        public virtual string[] GetRecipeInstructions()
        {
            var recipeInstructions = new List<string>();

            foreach (var formatScraper in formatScrapers)
            {
                if (formatScraper.IsActive)
                {
                    recipeInstructions = formatScraper.GetRecipeInstructions();
                    if (recipeInstructions.Count > 0)
                        break;
                }
            }

            return recipeInstructions.ToArray();
        }

        public virtual string GetRecipeLanguageISOCode()
        {
            return _pageContent.DocumentElement.GetAttribute("lang").ToString().Substring(0, 2);
        }
    }
}
