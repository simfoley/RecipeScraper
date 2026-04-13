using RecipeScraper.Parsers;
using RecipeScraper.UnitTests.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace RecipeScraper.UnitTests.Parsers;

public class HtmlTreeParserTests
{
    private static async Task<HtmlTreeParser> CreateParser(string html)
    {
        return new HtmlTreeParser(await DocumentHelper.CreateDocumentAsync(html));
    }

    // --- IsActive ---

    [Fact]
    public async Task IsActive_WithNoRecipeContent_ReturnsFalse()
    {
        // Deliberately short, no capital, no period — scores below 70 for both ingredient and instruction rules
        var html = "<html><body><p>just a simple webpage about travel</p></body></html>";
        var parser = await CreateParser(html);
        Assert.False(parser.IsActive);
    }

    [Fact]
    public async Task IsActive_WithIngredientLikeContent_ReturnsTrue()
    {
        var html = """
            <html><body>
              <ul>
                <li>2 cups butter</li>
                <li>1 kg flour</li>
                <li>3 ml olive oil</li>
              </ul>
            </body></html>
            """;
        var parser = await CreateParser(html);
        Assert.True(parser.IsActive);
    }

    [Fact]
    public async Task IsActive_WithInstructionLikeContent_ReturnsTrue()
    {
        var html = """
            <html><body>
              <ol>
                <li>Preheat oven to 350F. Mix all the ingredients together.</li>
                <li>Bake for 30 minutes until golden. Remove from oven and let cool.</li>
              </ol>
            </body></html>
            """;
        var parser = await CreateParser(html);
        Assert.True(parser.IsActive);
    }

    // --- GetRecipeIngredients ---

    [Fact]
    public async Task GetRecipeIngredients_WithIngredientLikeContent_ReturnsIngredients()
    {
        var html = """
            <html><body>
              <ul>
                <li>2 cups butter</li>
                <li>1 kg flour</li>
                <li>3 ml olive oil</li>
              </ul>
            </body></html>
            """;
        var parser = await CreateParser(html);
        var ingredients = parser.GetRecipeIngredients();
        Assert.NotEmpty(ingredients);
        Assert.Contains("2 cups butter", ingredients);
    }

    [Fact]
    public async Task GetRecipeIngredients_WithNoMatchingContent_ReturnsEmptyList()
    {
        var html = "<html><body><p>just a simple webpage about travel</p></body></html>";
        var parser = await CreateParser(html);
        Assert.Empty(parser.GetRecipeIngredients());
    }

    // --- GetRecipeInstructions ---

    [Fact]
    public async Task GetRecipeInstructions_WithInstructionLikeContent_ReturnsInstructions()
    {
        var step1 = "Preheat oven to 350F. Mix all the ingredients together in a large bowl.";
        var step2 = "Bake for 30 minutes until golden brown. Remove from oven and let cool.";
        var html = $"""
            <html><body>
              <ol>
                <li>{step1}</li>
                <li>{step2}</li>
              </ol>
            </body></html>
            """;
        var parser = await CreateParser(html);
        var instructions = parser.GetRecipeInstructions();
        Assert.NotEmpty(instructions);
        Assert.Contains(step1, instructions);
        Assert.Contains(step2, instructions);
    }

    [Fact]
    public async Task GetRecipeInstructions_WithNoMatchingContent_ReturnsEmptyList()
    {
        var html = "<html><body><p>just a simple webpage about travel</p></body></html>";
        var parser = await CreateParser(html);
        Assert.Empty(parser.GetRecipeInstructions());
    }

    // --- GetName ---

    [Fact]
    public async Task GetName_WithTitleTag_ReturnsTitle()
    {
        var html = "<html><head><title>My Recipe</title></head><body></body></html>";
        var parser = await CreateParser(html);
        Assert.Equal("My Recipe", parser.GetName());
    }

    [Fact]
    public async Task GetName_WithTitleAndOgTitle_ReturnsTitleFirst()
    {
        var html = """<html><head><title>Title Tag Recipe</title><meta property="og:title" content="OG Recipe"/></head><body></body></html>""";
        var parser = await CreateParser(html);
        // <title> takes priority over og:title
        Assert.Equal("Title Tag Recipe", parser.GetName());
    }
}
