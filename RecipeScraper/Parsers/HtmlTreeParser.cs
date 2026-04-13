using AngleSharp.Dom;
using RecipeScraper.Helpers;
using RecipeScraper.Parsers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeScraper.Parsers;

internal class HtmlTreeParser : IDocumentParser
{
    private struct NodeWithScore
    {
        public int Score { get; set; }
        public IElement Node { get; set; }
    }

    private const int ScoreThreshold = 70;
    private const int MinIngredientLength = 3;
    private const int MaxIngredientLength = 50;
    private const int MinInstructionLength = 20;
    private const int MaxInstructionLength = 500;

    private static readonly List<string> CommonIngredients = ["butter", "beurre", "olive oil", "huile d'olive", "salt", "sel", "pepper", "poivre"];
    private static readonly List<string> CommonUnits = ["cup", "tasse", "ml", "kg", "gram", "g ", "lb"];
    private static readonly List<string> InstructionsCommonWords = ["preheat", "préchauffer", "bring to a boil", "porter à ébullition", "bake", "cuire", "baking", "cuisson"];

    // Rules to award points to a text if it resembles a recipe ingredient
    private static readonly (Func<string, bool> Condition, int Points)[] IngredientRules =
    [
        (text => text.Length >= MinIngredientLength && text.Length < MaxIngredientLength, 30),
        (text => CommonIngredients.Any(i => text.Contains(i, StringComparison.CurrentCultureIgnoreCase)), 30),
        (text => CommonUnits.Any(u => text.Contains(u, StringComparison.CurrentCultureIgnoreCase)), 10),
        (text => char.IsDigit(text[0]), 20),
        (text => !text.Contains(". "), 10),
    ];

    // Rules to award points to a text if it resembles a recipe instruction
    private static readonly (Func<string, bool> Condition, int Points)[] InstructionRules =
    [
        (text => text.Length > MinInstructionLength && text.Length < MaxInstructionLength, 30),
        (text => InstructionsCommonWords.Any(w => text.Contains(w, StringComparison.CurrentCultureIgnoreCase)), 30),
        (text => char.IsUpper(text[0]), 10),
        (text => text.Contains(". "), 20),
        (text => char.IsPunctuation(text[text.Length - 1]), 10),
    ];

    private IDocument _pageContent;
    private IElement? _ingredientsParent = null;
    private IElement? _instructionsParent = null;
    private List<NodeWithScore> _ingredientNodes = new List<NodeWithScore>();
    private List<NodeWithScore> _instructionNodes = new List<NodeWithScore>();

    public bool IsActive => _ingredientNodes.Count > 0 || _instructionNodes.Count > 0;

    //Property that determines if the webpage is just a flat list of tags
    private bool IsFlatWebPage => _ingredientsParent != null && _ingredientsParent == _instructionsParent;

    public HtmlTreeParser(IDocument pageContent)
    {
        _pageContent = pageContent;
        FindIngredientAndInstructionNodes();
    }

    public string? GetName()
    {
        return _pageContent.Title
            ?? _pageContent.All.FirstOrDefault(x => x.HasAttribute("property") && x.GetAttribute("property")!.StartsWith("og:title"))?.GetAttribute("content");
    }

    public string? GetYield() => null;

    private static readonly string[] DecorativeImagePatterns = ["logo", "icon", "avatar", "sprite", ".svg"];

    public string? GetImage()
    {
        return _pageContent.Images
            .Where(img => !string.IsNullOrEmpty(img.Source))
            .Where(img => !DecorativeImagePatterns.Any(p => img.Source!.Contains(p, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(img => img.DisplayWidth * img.DisplayHeight)
            .FirstOrDefault()?.Source;
    }

    public TimeSpan? GetPrepTime() => null;

    public TimeSpan? GetCookTime() => null;

    public TimeSpan? GetTotalTime() => null;

    public List<string> GetRecipeIngredients()
    {
        var recipeIngredients = new List<string>();

        if (_ingredientNodes.Count > 0)
        {
            if (IsFlatWebPage)
            {
                recipeIngredients.AddRange(_ingredientNodes.Select(x => x.Node.TextContent));
            }
            else
            {
                recipeIngredients = AngleSharpHelpers.GetTextContentOfNodeRecursive(_ingredientsParent, 0);
            }
        }

        return recipeIngredients;

    }

    public List<string> GetRecipeInstructions()
    {
        var recipeInstructions = new List<string>();

        if (_instructionNodes.Count > 0)
        {
            if (IsFlatWebPage)
            {
                recipeInstructions.AddRange(_instructionNodes.Select(x => x.Node.TextContent));
            }
            else
            {
                recipeInstructions = AngleSharpHelpers.GetTextContentOfNodeRecursive(_instructionsParent, 0);
            }
        }

        return recipeInstructions;
    }

    private void FindIngredientAndInstructionNodes()
    {
        foreach (var node in _pageContent.All.Where(x => !string.IsNullOrEmpty(x.TextContent)))
        {
            string textContent = node.TextContent;
            var ingredientScore = CalculateTextIngredientScore(textContent);
            var instructionScore = CalculateTextInstructionScore(textContent);

            if (ingredientScore >= ScoreThreshold)
            {
                _ingredientNodes.Add(new NodeWithScore { Score = ingredientScore, Node = node });
            }

            if (instructionScore >= ScoreThreshold)
            {
                _instructionNodes.Add(new NodeWithScore { Score = instructionScore, Node = node });
            }
        }

        _ingredientNodes = _ingredientNodes.GroupBy(p => p.Node.TextContent).Select(g => g.First()).OrderByDescending(x => x.Score).ToList();
        _instructionNodes = _instructionNodes.GroupBy(p => p.Node.TextContent).Select(g => g.First()).OrderByDescending(x => x.Score).ToList();

        //If we have more than 1 node, try finding common parent for better results, otherwise, find first parent node that contains multiple children
        if (_ingredientNodes.Count > 1)
        {
            _ingredientsParent = AngleSharpHelpers.FindCommonParentNodeRecursive(_ingredientNodes[0].Node, _ingredientNodes[1].Node);
        }
        if (_ingredientsParent == null && _ingredientNodes.Count >= 1)
        {
            _ingredientsParent = AngleSharpHelpers.FindListParentNodeRecursive(_ingredientNodes[0].Node);
        }

        if (_instructionNodes.Count > 1)
        {
            _instructionsParent = AngleSharpHelpers.FindCommonParentNodeRecursive(_instructionNodes[0].Node, _instructionNodes[1].Node);
        }
        if (_instructionsParent == null && _instructionNodes.Count >= 1)
        {
            _instructionsParent = AngleSharpHelpers.FindListParentNodeRecursive(_instructionNodes[0].Node);
        }
    }

    // Returns a score out of 100 indicating the likelihood that the text is an ingredient
    private static int CalculateTextIngredientScore(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return IngredientRules.Where(r => r.Condition(text)).Sum(r => r.Points);
    }

    // Returns a score out of 100 indicating the likelihood that the text is an instruction
    private static int CalculateTextInstructionScore(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // If its a json, return a score of 0
        if (text.Contains('{') || text.Contains('}'))
            return 0;

        return InstructionRules.Where(r => r.Condition(text)).Sum(r => r.Points);
    }
}
