using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeScraper.Helpers
{
    internal static class AngleSharpHelpers
    {
        public static IElement GetSingleItemPropElementFromPageContent(string itemPropName, IDocument pageContent)
        {
            return pageContent.All.FirstOrDefault(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith(itemPropName));
        }

        public static List<IElement> GetMultipleItemPropElementsFromPageContent(string itemPropName, IDocument pageContent)
        {
            return pageContent.All.Where(x => x.HasAttribute("itemprop") && x.GetAttribute("itemprop").StartsWith(itemPropName)).ToList();
        }

        public static IElement FindListParentNodeRecursive(IElement node)
        {
            if (node.Children.Length > 1)
            {
                return node;
            }

            return FindListParentNodeRecursive(node.ParentElement);
        }

        public static IElement FindCommonParentNodeRecursive(IElement node1, IElement node2)
        {
            var parent1 = node1.ParentElement;
            var parent2 = node2.ParentElement;
            
            if (parent1 == null || parent2 == null)
                return null;
            
            if (parent1 == parent2)
                return node1.ParentElement;

            return FindCommonParentNodeRecursive(parent1, parent2);
        }

        public static List<string> GetTextContentOfNodeRecursive(IElement node, int recurseCount)
        {
            var textContents = new List<string>();

            if (node == null || recurseCount > 10)
                return textContents;

            recurseCount++;

            if (node.Children.Length == 0 && !string.IsNullOrEmpty(node.TextContent))
            {
                textContents.Add(node.TextContent);
            }
            else if (node.Children.Length > 0)
            {
                foreach (var childNode in node.Children)
                {
                    textContents.AddRange(GetTextContentOfNodeRecursive(childNode, recurseCount));
                }
            }

            return textContents;
        }
    }
}
