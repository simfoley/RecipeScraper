using System.Collections.Generic;
using System.Linq;

namespace RecipeScraper.Scrapers;

internal class LeCoupDeGraceScraper : RecipeScraperBase
{
    public override string? GetYield()
    {
        // Each info block shares the same outer class; identify the servings block by its icon
        var yieldElement = _pageContent.All.FirstOrDefault(x =>
            x.HasAttribute("class") && x.GetAttribute("class")!.Contains("single-recipe__infos--single") &&
            x.QuerySelector("[class*='icon-lecoupdegrace-serving']") != null);
        var contentElement = yieldElement?.Children.FirstOrDefault(x => x.HasAttribute("class") && x.GetAttribute("class")!.Contains("single-recipe__infos--content"));
        return contentElement?.Children.LastOrDefault()?.TextContent?.Trim();
    }
}
