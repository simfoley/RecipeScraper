using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RecipeScraper.Scrapers
{
    public class LeCoupDeGraceScraper : BaseScraper
    {
        public LeCoupDeGraceScraper(string url) : base(url)
        { 
        }

        public override string GetYield()
        {
            var yieldElement = _pageContent.All.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class").StartsWith("portions"));
            return yieldElement.Children.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class").StartsWith("info")).TextContent;
        }

        public override string[] GetRecipeInstructions()
        {
            var recipeInstructions = new List<string>();
            var instrctionsElement = _pageContent.All.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class").StartsWith("list-checked-wrap order-list"));
            foreach (var instruction in instrctionsElement.FirstElementChild.Children)
            {
                recipeInstructions.Add(instruction.TextContent);
            }
            return recipeInstructions.ToArray();
        }
    }
}
