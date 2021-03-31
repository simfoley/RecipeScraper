using RecipeScraper.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Text.Json;
using System.Xml;

namespace RecipeScraper.Scrapers
{
    //The BaseScraper should be able to handle pages compliant with http://schema.org/Recipe object, either with Json LD or with Microdata
    public class BaseScraper
    {
        string _url;
        IDocument _pageContent;
        JsonElement _jsonRecipe;
        IElement _microdata;

        public BaseScraper(string url)
        {
            _url = url;
            
            //Get page content
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            _pageContent = context.OpenAsync(url).Result;

            //Check if page uses json ld
            var jsonLdRecipeElement = _pageContent.Scripts.FirstOrDefault(x => x.Type == "application/ld+json" && x.InnerHtml.Contains("\"@type\": \"Recipe\""));
            if (jsonLdRecipeElement != null)
            {
                _jsonRecipe = JsonDocument.Parse(jsonLdRecipeElement.InnerHtml).RootElement;
            }

            //check if page uses microdata
            _microdata = _pageContent.All.FirstOrDefault(x => x.LocalName == "div" && x.HasAttribute("itemscope") && x.HasAttribute("itemtype") && x.GetAttribute("itemtype").StartsWith("http://schema.org/Recipe"));
        }

        public ScrapedRecipe ScrapeRecipe()
        {
            return new ScrapedRecipe() 
            { 
                Name = GetName(),
                Image = GetImage(),
                Yield = GetYield(),
                PrepTime = GetPrepTime(),
                CookTime = GetCookTime(),
                RecipeIngredients = GetRecipeIngredients(),
                RecipeInstructions = GetRecipeInstructions()
            };
        }

        public virtual string GetName()
        {
            return GetRecipeProperty("name");
        }

        public virtual string GetImage()
        {
            return GetRecipeProperty("image");
        }

        public virtual string GetYield()
        {
            return GetRecipeProperty("recipeYield");
        }

        public virtual TimeSpan GetPrepTime()
        {
            return XmlConvert.ToTimeSpan(GetRecipeProperty("prepTime"));
        }

        public virtual TimeSpan GetCookTime()
        {
            return XmlConvert.ToTimeSpan(GetRecipeProperty("cookTime"));
        }

        public virtual string[] GetRecipeIngredients() 
        {
            var recipeIngredients = new List<string>();

            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                foreach (var ingredient in _jsonRecipe.GetProperty("recipeIngredient").EnumerateArray())
                {
                    recipeIngredients.Add(ingredient.ToString());
                }
            }

            //Microdata
            if (recipeIngredients.Count == 0 && _microdata != null)
            {
                var ingredientsElement = _microdata.Children.FirstOrDefault(x => x.LocalName == "div" && !x.HasAttribute("itemprop") && x.InnerHtml.Contains("recipeIngredient"));
                if (ingredientsElement != null)
                {
                    foreach (var ingredient in ingredientsElement.Children.ToArray())
                    {
                        recipeIngredients.Add(ingredient.InnerHtml);
                    }
                }
            }

            return recipeIngredients.ToArray();
        }

        public virtual string[] GetRecipeInstructions()
        {
            var recipeInstructions = new List<string>();

            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                var instructionSection = _jsonRecipe.GetProperty("recipeInstructions");
                recipeInstructions.AddRange(GetRecipeInstrctionsRecursive(instructionSection));
            }

            //Microdata
            if (recipeInstructions.Count == 0 && _microdata != null)
            {
                var instructionsElement = _microdata.Children.FirstOrDefault(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith("recipeInstructions"));
                if (instructionsElement != null)
                {
                    foreach (var ingredient in instructionsElement.Children.ToArray())
                    {
                        recipeInstructions.Add(ingredient.InnerHtml);
                    }
                }
            }

            return recipeInstructions.ToArray();
        }

        protected string GetRecipeProperty(string propertyName)
        {
            string result = null;

            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                result = _jsonRecipe.GetProperty(propertyName).GetString();
            }

            //Microdata
            if (string.IsNullOrEmpty(result) && _microdata != null)
            {
                result = _microdata.Children.FirstOrDefault(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith(propertyName))?.InnerHtml;
            }

            return result;
        }

        //Because recipeInstruction can be in different formats in json-ld, just recurse through the json and keep the "text" properties values. 
        private List<string> GetRecipeInstrctionsRecursive(JsonElement element, int recurseCount=0)
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
