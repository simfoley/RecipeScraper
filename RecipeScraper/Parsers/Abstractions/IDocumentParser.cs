using System;
using System.Collections.Generic;

namespace RecipeScraper.Parsers.Abstractions;

internal interface IDocumentParser
{
    bool IsActive { get; }

    string? GetName();
    string? GetYield();
    string? GetImage();
    TimeSpan? GetPrepTime();
    TimeSpan? GetCookTime();
    List<string> GetRecipeIngredients();
    List<string> GetRecipeInstructions();
}
