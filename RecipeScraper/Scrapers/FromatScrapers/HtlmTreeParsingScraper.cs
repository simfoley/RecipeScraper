using AngleSharp.Dom;
using RecipeScraper.Helpers;
using RecipeScraper.Scrapers.FromatScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipeScraper.Scrapers.FormatScrapers
{
    internal class HtlmTreeParsingScraper : IFormatScraper
    {
        private struct NodeWithScore
        {
            public int Score { get; set; }
            public IElement Node { get; set; }
        }

        private List<string> CommonIngredients = new List<string> { "butter", "beurre", "olive oil", "huile d'olive", "salt", "sel", "pepper", "poivre" };
        private List<string> CommonUnits = new List<string> { "cup", "tasse", "ml", "kg", "gram", "g ", "lb" };
        private List<string> InstructionsCommonWords = new List<string> { "preheat", "préchauffer", "bring to a boil", "porter à ébullition", "bake", "cuire", "baking", "cuisson" };

        private IDocument _pageContent;
        private IElement _ingredientsParent = null;
        private IElement _instructionsParent = null;
        private List<NodeWithScore> _ingredientNodes = new List<NodeWithScore>();
        private List<NodeWithScore> _instructionNodes = new List<NodeWithScore>();

        public bool IsActive => _ingredientNodes.Count > 0 || _instructionNodes.Count > 0;

        //Property that determines if the webpage is just a flat list of tags
        private bool IsFlatWebPage => _ingredientsParent!= null && _ingredientsParent == _instructionsParent;

        public HtlmTreeParsingScraper(IDocument pageContent) 
        {
            _pageContent = pageContent;
            FindIngredientAndInstructionNodes();
        }

        public string GetName()
        {
            string name = null;

            name = _pageContent.Title;

            if (string.IsNullOrEmpty(name))
            {
                name = _pageContent.All.FirstOrDefault(x => x.HasAttribute("property") && x.GetAttribute("property").StartsWith("og:title"))?.GetAttribute("content");
            }

            return name;
        }

        public string GetYield()
        {
            return null;
        }

        public string GetImage()
        {
            return null;
        }

        public string GetPrepTime()
        {
            return string.Empty;
        }

        public string GetCookTime()
        {
            return string.Empty;
        }

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

                if (ingredientScore >= 70)
                {
                    _ingredientNodes.Add(new NodeWithScore { Score = ingredientScore, Node = node});
                }

                if (instructionScore >= 70)
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

        //Gives given text probability it is an ingredient on 100
        private int CalculateTextIngredientScore(string text)
        {
            int nodeScore = 0;

            if (text.Length < 50 && text.Length >= 3)
            {
                nodeScore += 30;
            }
            if (CommonIngredients.Any(ingredient => text.Contains(ingredient, StringComparison.CurrentCultureIgnoreCase)))
            {
                nodeScore += 30;
            }
            if (CommonUnits.Any(unit => text.Contains(unit, StringComparison.CurrentCultureIgnoreCase)))
            {
                nodeScore += 10;
            }
            if (Char.IsDigit(text[0]))
            {
                nodeScore += 20;
            }
            if (!text.Contains(". "))
            {
                nodeScore += 10;
            }

            return nodeScore;
        }

        //Gives given text probability it is an ingredient on 100
        private int CalculateTextInstructionScore(string text)
        {
            int nodeScore = 0;

            if (text.Length > 50 && text.Length < 500)
            {
                nodeScore += 30;
            }
            if (InstructionsCommonWords.Any(instruction => text.Contains(instruction, StringComparison.CurrentCultureIgnoreCase)))
            {
                nodeScore += 30;
            }
            if (Char.IsUpper(text[0]))
            {
                nodeScore += 10;
            }
            if (text.Contains(". "))
            {
                nodeScore += 20;
            }
            if (Char.IsPunctuation(text[text.Length - 1]))
            {
                nodeScore += 10;
            }
            if (text.Contains("{") || text.Contains("}"))
            {
                nodeScore = 0;
            }

            return nodeScore;
        }
    }
}
