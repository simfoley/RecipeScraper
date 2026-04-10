using AngleSharp.Dom;
using RecipeScraper.Parsers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace RecipeScraper.Parsers
{
    internal class JsonLdParser : IDocumentParser
    {
        private JsonElement _jsonRecipe;
        public bool IsActive => _jsonRecipe.ValueKind != JsonValueKind.Undefined;

        public JsonLdParser(IDocument pageContent)
        {
            foreach (var htmlElement in pageContent.Scripts.Where(x => x.Type == "application/ld+json" && (x.InnerHtml.Contains("\"@type\":\"Recipe\"") || x.InnerHtml.Contains("\"@type\": \"Recipe\""))))
            {
                var jsonElement = JsonDocument.Parse(htmlElement.InnerHtml).RootElement;


                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("@type", out JsonElement recipeType) && recipeType.GetString() == "Recipe")
                {
                    _jsonRecipe = jsonElement;
                    return;
                }
                else if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("@graph", out JsonElement graphType) && graphType.ValueKind == JsonValueKind.Array)
                {
                    foreach (var graphElement in jsonElement.GetProperty("@graph").EnumerateArray())
                    {
                        if (graphElement.ValueKind == JsonValueKind.Object && graphElement.TryGetProperty("@type", out JsonElement graphElementType) && graphElementType.GetString() == "Recipe")
                        {
                            _jsonRecipe = graphElement;
                            return;
                        }
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var innerJsonElement in jsonElement.EnumerateArray())
                    {
                        if (innerJsonElement.ValueKind == JsonValueKind.Object && innerJsonElement.TryGetProperty("@type", out JsonElement innerType) && innerType.GetString() == "Recipe")
                        {
                            _jsonRecipe = innerJsonElement;
                            return;
                        }
                    }
                }
            }
        }

        public string? GetName()
        {
            if (_jsonRecipe.TryGetProperty("name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                return nameElement.GetString();
            }

            return null;
        }

        public string? GetImage()
        {
            if (_jsonRecipe.TryGetProperty("image", out JsonElement imageElement))
            {
                switch (imageElement.ValueKind)
                {
                    case JsonValueKind.Array:
                        return imageElement[0].GetString();
                    case JsonValueKind.String:
                        return imageElement.GetString();
                    case JsonValueKind.Object:
                        {
                            if (imageElement.TryGetProperty("url", out JsonElement url) && url.ValueKind == JsonValueKind.String)
                            {
                                return url.GetString();
                            }

                            break;
                        }
                }
            }

            return null;
        }

        public string? GetYield()
        {
            if (_jsonRecipe.TryGetProperty("recipeYield", out JsonElement yieldElement) && yieldElement.ValueKind == JsonValueKind.String)
            {
                return yieldElement.GetString();
            }

            return null;
        }

        public TimeSpan? GetPrepTime()
        {
            if (_jsonRecipe.TryGetProperty("prepTime", out JsonElement prepTimeElement) && prepTimeElement.ValueKind == JsonValueKind.String)
            {
                string? prepTime = prepTimeElement.GetString();
                if (!string.IsNullOrEmpty(prepTime))
                    return XmlConvert.ToTimeSpan(prepTime);
            }

            return null;
        }

        public TimeSpan? GetCookTime()
        {
            if (_jsonRecipe.TryGetProperty("cookTime", out JsonElement cookTimeElement) && cookTimeElement.ValueKind == JsonValueKind.String)
            {
                string? cookTime = cookTimeElement.GetString();
                if (!string.IsNullOrEmpty(cookTime))
                    return XmlConvert.ToTimeSpan(cookTime);
            }

            return null;
        }

        public List<string> GetRecipeIngredients()
        {
            var recipeIngredients = new List<string>();

            if (_jsonRecipe.TryGetProperty("recipeIngredient", out JsonElement recipeIngredientsElement) && recipeIngredientsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var ingredient in recipeIngredientsElement.EnumerateArray())
                {
                    if (ingredient.ValueKind == JsonValueKind.String)
                    {
                        recipeIngredients.Add(ingredient.ToString());
                    }
                }
            }

            return recipeIngredients;
        }

        public List<string> GetRecipeInstructions()
        {
            if (_jsonRecipe.TryGetProperty("recipeInstructions", out JsonElement recipeInstructionsElement))
            {
                return GetRecipeInstructionsRecursive(recipeInstructionsElement);
            }

            return new List<string>();
        }

        //Because recipeInstruction can be in different formats in json-ld, just recurse through the json and keep the "text" properties values. 
        private List<string> GetRecipeInstructionsRecursive(JsonElement element, int recurseCount = 0)
        {
            var result = new List<string>();

            if (recurseCount > 10)
                return result;

            recurseCount++;

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    result.AddRange(GetRecipeInstructionsRecursive(item, recurseCount));
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in element.EnumerateObject())
                {
                    if (item.Value.ValueKind == JsonValueKind.Object || item.Value.ValueKind == JsonValueKind.Array)
                    {
                        result.AddRange(GetRecipeInstructionsRecursive(item.Value, recurseCount));
                    }
                    else if (item.Name == "text")
                    {
                        result.Add(item.Value.ToString());
                    }
                }
            }

            return result;
        }
    }
}
