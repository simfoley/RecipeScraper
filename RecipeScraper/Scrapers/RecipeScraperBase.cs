using AngleSharp;
using AngleSharp.Dom;
using RecipeScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecipeScraper.Scrapers.Abstractions;
using RecipeScraper.Parsers.Abstractions;
using RecipeScraper.Parsers;

namespace RecipeScraper.Scrapers;

// Handles pages compliant with http://schema.org/Recipe via Json-LD, Microdata, or HTML tree parsing
public class RecipeScraperBase : IRecipeScraper
{
    protected IDocument _pageContent = null!;
    private List<IDocumentParser> _documentParsers = new();

    public async Task<ScrapedRecipe> ScrapeRecipe(string url)
    {
        await LoadPageDocument(url);
        InitializeFormatScrapers();

        return new ScrapedRecipe
        {
            Name = GetName(),
            Image = GetImage(),
            Yield = GetYield(),
            PrepTime = GetPrepTime(),
            CookTime = GetCookTime(),
            TotalTime = GetTotalTime(),
            RecipeIngredients = GetRecipeIngredients(),
            RecipeInstructions = GetRecipeInstructions(),
            RecipeLanguageISOCode = GetRecipeLanguageISOCode()
        };
    }

    private async Task LoadPageDocument(string url)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        _pageContent = await context.OpenAsync(url);
    }

    private void InitializeFormatScrapers()
    {
        _documentParsers = new List<IDocumentParser>
        {
            new JsonLdParser(_pageContent),
            new MicrodataParser(_pageContent),
            new HtmlTreeParser(_pageContent)
        };

        if (!_documentParsers.Any(f => f.IsActive))
            throw new NotSupportedException("This webpage does not contain a schema.org recipe object to scrape.");
    }

    public virtual string? GetName()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var name = formatScraper.GetName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
        }

        return null;
    }

    public virtual string? GetImage()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var image = formatScraper.GetImage();
                if (!string.IsNullOrEmpty(image))
                    return image;
            }
        }

        return null;
    }

    public virtual string? GetYield()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var yield = formatScraper.GetYield();
                if (!string.IsNullOrEmpty(yield))
                    return yield;
            }
        }

        return null;
    }

    public virtual TimeSpan? GetPrepTime()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var prepTime = formatScraper.GetPrepTime();
                if (prepTime != null)
                    return prepTime;
            }
        }

        return null;
    }

    public virtual TimeSpan? GetCookTime()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var cookTime = formatScraper.GetCookTime();
                if (cookTime != null)
                    return cookTime;
            }
        }

        return null;
    }

    public virtual TimeSpan? GetTotalTime()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var totalTime = formatScraper.GetTotalTime();
                if (totalTime != null)
                    return totalTime;
            }
        }

        return null;
    }

    public virtual string[] GetRecipeIngredients()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var ingredients = formatScraper.GetRecipeIngredients();
                if (ingredients.Count > 0)
                    return ingredients.ToArray();
            }
        }

        return Array.Empty<string>();
    }

    public virtual string[] GetRecipeInstructions()
    {
        foreach (var formatScraper in _documentParsers)
        {
            if (formatScraper.IsActive)
            {
                var instructions = formatScraper.GetRecipeInstructions();
                if (instructions.Count > 0)
                    return instructions.ToArray();
            }
        }

        return Array.Empty<string>();
    }

    public virtual string? GetRecipeLanguageISOCode()
    {
        return _pageContent.DocumentElement.GetAttribute("lang")?.Substring(0, 2);
    }
}
