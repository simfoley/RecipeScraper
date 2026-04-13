using RecipeScraper.Parsers;
using RecipeScraper.UnitTests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RecipeScraper.UnitTests.Parsers;

public class MicrodataParserTests
{
    private static async Task<MicrodataParser> CreateParser(string html)
    {
        return new MicrodataParser(await DocumentHelper.CreateDocumentAsync(html));
    }

    // --- IsActive ---

    [Fact]
    public async Task IsActive_WithNoItemprop_ReturnsFalse()
    {
        var parser = await CreateParser("<html><body><p>No microdata here.</p></body></html>");
        Assert.False(parser.IsActive);
    }

    [Fact]
    public async Task IsActive_WithItemprop_ReturnsTrue()
    {
        var parser = await CreateParser("""<html><body><span itemprop="name">Test</span></body></html>""");
        Assert.True(parser.IsActive);
    }

    // --- GetName ---

    [Fact]
    public async Task GetName_SimpleItemprop_ReturnsName()
    {
        var parser = await CreateParser("""<html><body><span itemprop="name">Pasta Carbonara</span></body></html>""");
        Assert.Equal("Pasta Carbonara", parser.GetName());
    }

    [Fact]
    public async Task GetName_MultipleNames_ReturnsRecipeName()
    {
        var html = """
            <html><body>
              <div itemtype="http://schema.org/Recipe">
                <span itemprop="name">Pasta Carbonara</span>
              </div>
              <div itemtype="http://schema.org/Person">
                <span itemprop="name">John Doe</span>
              </div>
            </body></html>
            """;
        var parser = await CreateParser(html);
        Assert.Equal("Pasta Carbonara", parser.GetName());
    }

    // --- GetYield ---

    [Fact]
    public async Task GetYield_AsTextContent_ReturnsYield()
    {
        var parser = await CreateParser("""<html><body><span itemprop="recipeYield">4 servings</span></body></html>""");
        Assert.Equal("4 servings", parser.GetYield());
    }

    [Fact]
    public async Task GetYield_AsMetaElement_ReturnsContent()
    {
        var parser = await CreateParser("""<html><body><meta itemprop="recipeYield" content="6"/></body></html>""");
        Assert.Equal("6", parser.GetYield());
    }

    // --- GetImage ---

    [Fact]
    public async Task GetImage_AsImgSrc_ReturnsImageUrl()
    {
        var parser = await CreateParser("""<html><body><img itemprop="image" src="https://example.com/image.jpg"/></body></html>""");
        Assert.Equal("https://example.com/image.jpg", parser.GetImage());
    }

    // --- GetPrepTime ---

    [Fact]
    public async Task GetPrepTime_AsTimeElement_ReturnsPrepTime()
    {
        var parser = await CreateParser("""<html><body><time itemprop="prepTime" datetime="PT15M">15 min</time></body></html>""");
        Assert.Equal(TimeSpan.FromMinutes(15), parser.GetPrepTime());
    }

    [Fact]
    public async Task GetPrepTime_AsMetaElement_ReturnsPrepTime()
    {
        var parser = await CreateParser("""<html><body><meta itemprop="prepTime" content="PT30M"/></body></html>""");
        Assert.Equal(TimeSpan.FromMinutes(30), parser.GetPrepTime());
    }

    [Fact]
    public async Task GetPrepTime_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""<html><body><span itemprop="name">Test</span></body></html>""");
        Assert.Null(parser.GetPrepTime());
    }

    // --- GetCookTime ---

    [Fact]
    public async Task GetCookTime_AsTimeElement_ReturnsCookTime()
    {
        var parser = await CreateParser("""<html><body><time itemprop="cookTime" datetime="PT1H">1 hour</time></body></html>""");
        Assert.Equal(TimeSpan.FromHours(1), parser.GetCookTime());
    }

    [Fact]
    public async Task GetCookTime_AsMetaElement_ReturnsCookTime()
    {
        var parser = await CreateParser("""<html><body><meta itemprop="cookTime" content="PT45M"/></body></html>""");
        Assert.Equal(TimeSpan.FromMinutes(45), parser.GetCookTime());
    }

    // --- GetRecipeIngredients ---

    [Fact]
    public async Task GetRecipeIngredients_ReturnsIngredients()
    {
        var html = """
            <html><body>
              <span itemprop="recipeIngredient">2 cups flour</span>
              <span itemprop="recipeIngredient">1 tsp salt</span>
              <span itemprop="recipeIngredient">3 eggs</span>
            </body></html>
            """;
        var parser = await CreateParser(html);
        var ingredients = parser.GetRecipeIngredients();
        Assert.Equal(3, ingredients.Count);
        Assert.Contains("2 cups flour", ingredients);
    }

    [Fact]
    public async Task GetRecipeIngredients_FallsBackToIngredientsProperty()
    {
        var html = """
            <html><body>
              <span itemprop="ingredients">3 eggs</span>
              <span itemprop="ingredients">100g butter</span>
            </body></html>
            """;
        var parser = await CreateParser(html);
        var ingredients = parser.GetRecipeIngredients();
        Assert.Equal(2, ingredients.Count);
    }

    [Fact]
    public async Task GetRecipeIngredients_MissingField_ReturnsEmptyList()
    {
        var parser = await CreateParser("""<html><body><span itemprop="name">Test</span></body></html>""");
        Assert.Empty(parser.GetRecipeIngredients());
    }

    // --- GetRecipeInstructions ---

    [Fact]
    public async Task GetRecipeInstructions_ReturnsInstructions()
    {
        var html = """
            <html><body>
              <div itemprop="recipeInstructions">
                <p>Preheat oven to 350F.</p>
                <p>Mix all ingredients together.</p>
              </div>
            </body></html>
            """;
        var parser = await CreateParser(html);
        var instructions = parser.GetRecipeInstructions();
        Assert.NotEmpty(instructions);
        Assert.Contains("Preheat oven to 350F.", instructions);
    }
}
