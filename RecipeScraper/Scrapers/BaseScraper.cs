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
using System.Text.RegularExpressions;

namespace RecipeScraper.Scrapers
{
    //The BaseScraper should be able to handle pages compliant with http://schema.org/Recipe object, either with Json LD or with Microdata or both
    public class BaseScraper
    {
        protected string _url;
        protected IDocument _pageContent;
        protected JsonElement _jsonRecipe;

        public BaseScraper(string url)
        {
            _url = url;
            
            //Get page content
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            _pageContent = context.OpenAsync(url).Result;
            
            //Get recipe Json LD if possible
            SetRecipeJson();
        }

        public bool PageContentIsValid()
        {
            if (_jsonRecipe.ValueKind == JsonValueKind.Undefined)
            {
                if (_pageContent.All.FirstOrDefault(x => x.HasAttribute("itemprop")) == null)
                {
                    return false;
                }
            }

            return true;
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
                RecipeInstructions = IgnoreExceptions(() => GetRecipeInstructions(), new string[0])
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
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                if (_jsonRecipe.GetProperty("name").ValueKind == JsonValueKind.String)
                {
                    name = _jsonRecipe.GetProperty("name").GetString();
                }
            }

            //Microdata
            if (string.IsNullOrEmpty(name))
            {
                var nameElements = GetMultipleItemPropElements("name");
                if (nameElements.Count > 1)
                {
                    name = nameElements.FirstOrDefault(x => x.ParentElement.HasAttribute("itemtype") && x.ParentElement.GetAttribute("itemtype").StartsWith("http://schema.org/Recipe")).TextContent.Trim();
                }
                else if (nameElements.Count == 1)
                {
                    name = nameElements.First().TextContent.Trim();
                }
            }

            name = Regex.Replace(name, @"\r\n?|\n", "");

            return name;
        }

        public virtual string GetImage()
        {
            string imageSource = null;
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                var imageProperty = _jsonRecipe.GetProperty("image");
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
            }

            //Microdata
            if (string.IsNullOrEmpty(imageSource))
            {
                IElement imageElement = null;
                var imageElements = GetMultipleItemPropElements("image");
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
            }

            return imageSource;
        }

        public virtual string GetYield()
        {
            string yield = null;
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                if (_jsonRecipe.GetProperty("recipeYield").ValueKind == JsonValueKind.String)
                {
                    yield = _jsonRecipe.GetProperty("recipeYield").GetString();
                }
            }

            //Microdata
            if (string.IsNullOrEmpty(yield))
            {
                yield = GetSingleItemPropElement("recipeYield")?.TextContent.Trim();
            }

            return yield;
        }

        public virtual TimeSpan GetPrepTime()
        {
            string prepTime = string.Empty;
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                if (_jsonRecipe.GetProperty("prepTime").ValueKind == JsonValueKind.String)
                {
                    prepTime = _jsonRecipe.GetProperty("prepTime").GetString();
                }
            }

            //Microdata
            if (string.IsNullOrEmpty(prepTime))
            {
                var prepTimeElement = GetSingleItemPropElement("prepTime");
                if (prepTimeElement.LocalName == "time")
                {
                    prepTime = prepTimeElement.GetAttribute("datetime");
                }
                else
                {
                    prepTime = prepTimeElement.InnerHtml.Trim();
                }
            }

            return XmlConvert.ToTimeSpan(prepTime);
        }

        public virtual TimeSpan GetCookTime()
        {
            string cookTime = string.Empty;
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                if (_jsonRecipe.GetProperty("cookTime").ValueKind == JsonValueKind.String)
                {
                    cookTime = _jsonRecipe.GetProperty("cookTime").GetString();
                }
            }

            //Microdata
            if (string.IsNullOrEmpty(cookTime))
            {
                var cookTimeElement = GetSingleItemPropElement("cookTime");
                if (cookTimeElement.LocalName == "time")
                {
                    cookTime = cookTimeElement.GetAttribute("datetime");
                }
                else
                {
                    cookTime = cookTimeElement.InnerHtml.Trim();
                }
            }

            return XmlConvert.ToTimeSpan(cookTime);
        }

        public virtual string[] GetRecipeIngredients() 
        {
            var recipeIngredients = new List<string>();
            
            //Json LD
            if (_jsonRecipe.ValueKind != JsonValueKind.Undefined)
            {
                foreach (var ingredient in _jsonRecipe.GetProperty("recipeIngredient").EnumerateArray())
                {
                    if (ingredient.ValueKind == JsonValueKind.String)
                    {
                        recipeIngredients.Add(ingredient.ToString());
                    }
                }
            }

            //Microdata
            if (!recipeIngredients.Any())
            {
                var ingredientElements = GetMultipleItemPropElements("recipeIngredient");
                if (!ingredientElements.Any())
                {
                    ingredientElements = GetMultipleItemPropElements("ingredients");
                }

                foreach (var ingredient in ingredientElements)
                {
                    recipeIngredients.Add(ingredient.TextContent.Trim());
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
            if (!recipeInstructions.Any())
            {
                var instructionsElement = GetSingleItemPropElement("recipeInstructions");
                if (instructionsElement != null)
                {
                    //Check current element is the list already or one children deeper
                    if (instructionsElement.FirstElementChild.Children.Count() == 0)
                    {
                        foreach (var ingredient in instructionsElement.Children.ToArray())
                        {
                            recipeInstructions.Add(ingredient.TextContent.Trim());
                        }
                    }
                    else
                    {
                        foreach (var ingredient in instructionsElement.FirstElementChild.Children.ToArray())
                        {
                            recipeInstructions.Add(ingredient.TextContent.Trim());
                        }
                    }
                }
            }

            return recipeInstructions.ToArray();
        }

        protected IElement GetSingleItemPropElement(string itemPropName)
        {
            return _pageContent.All.FirstOrDefault(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith(itemPropName));
        }

        protected List<IElement> GetMultipleItemPropElements(string itemPropName)
        {
            return _pageContent.All.Where(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith(itemPropName)).ToList();
        }

        private void SetRecipeJson()
        {
            var jsonLdRecipeElement = _pageContent.Scripts.FirstOrDefault(x => x.Type == "application/ld+json" && x.InnerHtml.Contains("\"@type\": \"Recipe\""));
            if (jsonLdRecipeElement != null)
            {
                _jsonRecipe = JsonDocument.Parse(jsonLdRecipeElement.InnerHtml).RootElement;
            }
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
