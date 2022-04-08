using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeScraper.Scrapers.FromatScrapers
{
    internal interface IFormatScraper
    {
        bool IsActive { get; }

        string GetName();
        string GetYield();
        string GetImage();
        string GetPrepTime();
        string GetCookTime();
        List<string> GetRecipeIngredients();
        List<string> GetRecipeInstructions();
    }
}
