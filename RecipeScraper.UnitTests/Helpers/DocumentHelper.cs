using AngleSharp;
using AngleSharp.Dom;
using System.Threading.Tasks;

namespace RecipeScraper.UnitTests.Helpers;

internal static class DocumentHelper
{
    public static async Task<IDocument> CreateDocumentAsync(string html)
    {
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html));
    }
}
