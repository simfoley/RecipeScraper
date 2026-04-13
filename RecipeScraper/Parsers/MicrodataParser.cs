using AngleSharp.Dom;
using RecipeScraper.Helpers;
using RecipeScraper.Parsers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace RecipeScraper.Parsers
{
    internal class MicrodataParser : IDocumentParser
    {
        public bool IsActive => _pageContent.All.FirstOrDefault(x => x.HasAttribute("itemprop")) != null;
        private IDocument _pageContent;

        public MicrodataParser(IDocument pageContent)
        {
            _pageContent = pageContent;
        }

        public string? GetName()
        {
            var nameElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("name", _pageContent);

            return nameElements.Count <= 1
                ? nameElements.FirstOrDefault()?.TextContent.Trim()
                : nameElements.FirstOrDefault(x => x.ParentElement != null && x.ParentElement.HasAttribute("itemtype") && x.ParentElement.GetAttribute("itemtype")!.StartsWith("http://schema.org/Recipe"))?.TextContent.Trim();
        }

        public string? GetYield()
        {
            var yieldElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("recipeYield", _pageContent);

            return yieldElement?.LocalName == "meta"
                ? yieldElement.GetAttribute("content")
                : yieldElement?.TextContent.Trim();
        }

        public string? GetImage()
        {
            var imageElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("image", _pageContent);

            IElement? imageElement = imageElements.Count <= 1
                ? imageElements.FirstOrDefault()
                : imageElements.FirstOrDefault(x => x.ParentElement != null && x.ParentElement.HasAttribute("itemtype") && x.ParentElement.GetAttribute("itemtype")!.StartsWith("http://schema.org/Recipe"));

            return imageElement?.LocalName == "img"
                ? imageElement.GetAttribute("src")
                : imageElement?.TextContent.Trim();
        }

        public TimeSpan? GetPrepTime()
        {
            var prepTimeElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("prepTime", _pageContent);

            string? prepTime = prepTimeElement?.LocalName switch
            {
                "time" => prepTimeElement.GetAttribute("datetime"),
                "meta" => prepTimeElement.GetAttribute("content"),
                _ => prepTimeElement?.TextContent.Trim()
            };

            return prepTime != null ? XmlConvert.ToTimeSpan(prepTime) : null;
        }

        public TimeSpan? GetCookTime()
        {
            var cookTimeElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("cookTime", _pageContent);

            string? cookTime = cookTimeElement?.LocalName switch
            {
                "time" => cookTimeElement.GetAttribute("datetime"),
                "meta" => cookTimeElement.GetAttribute("content"),
                _ => cookTimeElement?.TextContent.Trim()
            };

            return cookTime != null ? XmlConvert.ToTimeSpan(cookTime) : null;
        }

        public TimeSpan? GetTotalTime()
        {
            var totalTimeElement = AngleSharpHelpers.GetSingleItemPropElementFromPageContent("totalTime", _pageContent);

            string? totalTime = totalTimeElement?.LocalName switch
            {
                "time" => totalTimeElement.GetAttribute("datetime"),
                "meta" => totalTimeElement.GetAttribute("content"),
                _ => totalTimeElement?.TextContent.Trim()
            };

            return totalTime != null ? XmlConvert.ToTimeSpan(totalTime) : null;
        }

        public List<string> GetRecipeIngredients()
        {
            var ingredientElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("recipeIngredient", _pageContent);

            if (!ingredientElements.Any())
            {
                ingredientElements = AngleSharpHelpers.GetMultipleItemPropElementsFromPageContent("ingredients", _pageContent);
            }

            return ingredientElements
                .Select(x => x.TextContent.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
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
