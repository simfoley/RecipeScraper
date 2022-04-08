using AngleSharp.Dom;
using RecipeScraper.Helpers;
using RecipeScraper.Scrapers.FromatScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipeScraper.Scrapers.FormatScrapers
{
    internal class MircrodataScraper : IFormatScraper
    {
        public bool IsActive => _pageContent.All.FirstOrDefault(x => x.HasAttribute("itemprop")) != null;
        private IDocument _pageContent;

        public MircrodataScraper(IDocument pageContent)
        {
            _pageContent = pageContent;
        }

        public string GetName()
        {
            string name = null;

            var nameElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("name", _pageContent);
            if (nameElements.Count > 1)
            {
                name = nameElements.FirstOrDefault(x => x.ParentElement.HasAttribute("itemtype") && x.ParentElement.GetAttribute("itemtype").StartsWith("http://schema.org/Recipe")).TextContent.Trim();
            }
            else if (nameElements.Count == 1)
            {
                name = nameElements.First().TextContent.Trim();
            }

            return name;
        }

        public string GetYield()
        {
            string yield = null;

            var yieldElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("recipeYield", _pageContent);
            if (yieldElement?.LocalName == "meta")
            {
                yield = yieldElement.GetAttribute("content");
            }
            else
            {
                yield = yieldElement?.TextContent.Trim();
            }

            return yield;
        }

        public string GetImage()
        {
            string imageSource = null;

            IElement imageElement = null;
            var imageElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("image", _pageContent);
            if (imageElements.Count == 1)
            {
                imageElement = imageElements.First();
            }
            else
            {
                imageElement = imageElements.FirstOrDefault(x => x.ParentElement.HasAttribute("itemtype") && x.ParentElement.GetAttribute("itemtype").StartsWith("http://schema.org/Recipe"));
            }

            if (imageElement?.LocalName == "img")
            {
                imageSource = imageElement.GetAttribute("src");
            }
            else if (imageElement != null)
            {
                imageSource = imageElement.TextContent.Trim();
            }

            return imageSource;
        }

        public string GetPrepTime()
        {
            string prepTime = string.Empty;

            var prepTimeElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("prepTime", _pageContent);
            if (prepTimeElement?.LocalName == "time")
            {
                prepTime = prepTimeElement.GetAttribute("datetime");
            }
            else if (prepTimeElement?.LocalName == "meta")
            {
                prepTime = prepTimeElement.GetAttribute("content");
            }
            else
            {
                prepTime = prepTimeElement?.InnerHtml.Trim();
            }

            return prepTime;
        }

        public string GetCookTime()
        {
            string cookTime = string.Empty;

            var cookTimeElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("cookTime", _pageContent);
            if (cookTimeElement?.LocalName == "time")
            {
                cookTime = cookTimeElement.GetAttribute("datetime");
            }
            else if (cookTimeElement?.LocalName == "meta")
            {
                cookTime = cookTimeElement.GetAttribute("content");
            }
            else
            {
                cookTime = cookTimeElement?.InnerHtml.Trim();
            }

            return cookTime;
        }

        public List<string> GetRecipeIngredients()
        {
            var recipeIngredients = new List<string>();
           
            var ingredientElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("recipeIngredient", _pageContent);
            if (!ingredientElements.Any())
            {
                ingredientElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("ingredients", _pageContent);
            }

            foreach (var ingredient in ingredientElements)
            {
                recipeIngredients.Add(ingredient.TextContent.Trim());
            }

            return recipeIngredients;
        }

        public List<string> GetRecipeInstructions()
        {
            var recipeInstructions = new List<string>();

            var instructionsElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("recipeInstructions", _pageContent);
            foreach (var instructionsElement in instructionsElements)
            {
                recipeInstructions.AddRange(AngleSharpHelpers.GetTextContentOfNodeRecursive(instructionsElement, 0));
            }

            return recipeInstructions;
        }
    }
}
