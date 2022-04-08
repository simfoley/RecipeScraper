using AngleSharp.Dom;
using RecipeScraper.Scrapers.FromatScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace RecipeScraper.Scrapers.FormatScrapers
{
    internal class JsonLdScraper : IFormatScraper
    {
        private JsonElement _jsonRecipe;
        public bool IsActive => _jsonRecipe.ValueKind != JsonValueKind.Undefined;
        
        public JsonLdScraper(IDocument pageContent)
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

        public string GetName()
        {
            string name = null;
            
            if (_jsonRecipe.GetProperty("name").ValueKind == JsonValueKind.String)
            {
                name = _jsonRecipe.GetProperty("name").GetString();
            }

            return name;
        }

        public string GetImage()
        {
            string imageSource = null;

            var imageProperty = _jsonRecipe.GetProperty("image");
            if (imageProperty.ValueKind == JsonValueKind.Array)
            {
                imageSource = imageProperty[0].GetString();
            }
            if (imageProperty.ValueKind == JsonValueKind.Object)
            {
                if (imageProperty.GetProperty("url").ValueKind == JsonValueKind.String)
                {
                    imageSource = imageProperty.GetProperty("url").GetString();
                }
            }
            else if (imageProperty.ValueKind == JsonValueKind.String)
            {
                imageSource = imageProperty.GetString();
            }

            return imageSource;
        }

        public string GetYield()
        {
            string yield = null;
            
            if (_jsonRecipe.GetProperty("recipeYield").ValueKind == JsonValueKind.String)
            {
                yield = _jsonRecipe.GetProperty("recipeYield").GetString();
            }

            return yield;
        }

        public string GetPrepTime()
        {
            string prepTime = string.Empty;

            //Json LD
            if (_jsonRecipe.GetProperty("prepTime").ValueKind == JsonValueKind.String)
            {
                prepTime = _jsonRecipe.GetProperty("prepTime").GetString();
            }

            return prepTime;
        }

        public string GetCookTime()
        {
            string cookTime = string.Empty;
            
            if (_jsonRecipe.GetProperty("cookTime").ValueKind == JsonValueKind.String)
            {
                cookTime = _jsonRecipe.GetProperty("cookTime").GetString();
            }

            return cookTime;
        }

        public List<string> GetRecipeIngredients()
        {
            var recipeIngredients = new List<string>();
            
            foreach (var ingredient in _jsonRecipe.GetProperty("recipeIngredient").EnumerateArray())
            {
                if (ingredient.ValueKind == JsonValueKind.String)
                {
                    recipeIngredients.Add(ingredient.ToString());
                }
            }

            return recipeIngredients;
        }

        public List<string> GetRecipeInstructions()
        {
            var recipeInstructions = new List<string>();

            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                var instructionSection = _jsonRecipe.GetProperty("recipeInstructions");
                recipeInstructions.AddRange(GetRecipeInstrctionsRecursive(instructionSection));
            }

            return recipeInstructions;
        }

        //Because recipeInstruction can be in different formats in json-ld, just recurse through the json and keep the "text" properties values. 
        private List<string> GetRecipeInstrctionsRecursive(JsonElement element, int recurseCount = 0)
        {
            var result = new List<string>();

            if (recurseCount > 10)
                return result;

            recurseCount++;

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    result.AddRange(GetRecipeInstrctionsRecursive(item, recurseCount));
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in element.EnumerateObject())
                {
                    if (item.Value.ValueKind == JsonValueKind.Object || item.Value.ValueKind == JsonValueKind.Array)
                    {
                        result.AddRange(GetRecipeInstrctionsRecursive(item.Value, recurseCount));
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
