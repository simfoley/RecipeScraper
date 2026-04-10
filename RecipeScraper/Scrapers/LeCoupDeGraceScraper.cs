using System.Collections.Generic;
using System.Linq;

namespace RecipeScraper.Scrapers;

internal class LeCoupDeGraceScraper : RecipeScraperBase
{
    public override string? GetYield()
    {
        var yieldElement = _pageContent.All.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class")!.StartsWith("portions"));
        return yieldElement!.Children.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class")!.StartsWith("info"))?.TextContent;
    }

    public override string[] GetRecipeInstructions()
    {
        var recipeInstructions = new List<string>();
        var instructionsElement = _pageContent.All.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class")!.StartsWith("list-checked-wrap order-list"));
        foreach (var instruction in instructionsElement!.FirstElementChild!.Children)
        {
            recipeInstructions.Add(instruction.TextContent);
        }
        return recipeInstructions.ToArray();
    }
}
