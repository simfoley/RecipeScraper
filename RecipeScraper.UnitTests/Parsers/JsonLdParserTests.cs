using RecipeScraper.Parsers;
using RecipeScraper.UnitTests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RecipeScraper.UnitTests.Parsers;

public class JsonLdParserTests
{
    private static async Task<JsonLdParser> CreateParser(string jsonLd)
    {
        var html = $"""
            <html><body>
            <script type="application/ld+json">{jsonLd}</script>
            </body></html>
            """;
        return new JsonLdParser(await DocumentHelper.CreateDocumentAsync(html));
    }

    // --- IsActive ---

    [Fact]
    public async Task IsActive_WithNoJsonLd_ReturnsFalse()
    {
        var parser = new JsonLdParser(await DocumentHelper.CreateDocumentAsync("<html><body></body></html>"));
        Assert.False(parser.IsActive);
    }

    [Fact]
    public async Task IsActive_WithRecipeJsonLd_ReturnsTrue()
    {
        var parser = await CreateParser("""{"@type":"Recipe","name":"Test"}""");
        Assert.True(parser.IsActive);
    }

    [Fact]
    public async Task IsActive_WithNonRecipeJsonLd_ReturnsFalse()
    {
        var parser = await CreateParser("""{"@type":"Article","name":"Test"}""");
        Assert.False(parser.IsActive);
    }

    // --- GetName ---

    [Fact]
    public async Task GetName_ReturnsName()
    {
        var parser = await CreateParser("""{"@type":"Recipe","name":"Chocolate Cake"}""");
        Assert.Equal("Chocolate Cake", parser.GetName());
    }

    [Fact]
    public async Task GetName_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Null(parser.GetName());
    }

    [Fact]
    public async Task GetName_RecipeInsideGraph_ReturnsName()
    {
        var parser = await CreateParser("""{"@context":"https://schema.org","@graph":[{"@type":"WebPage"},{"@type":"Recipe","name":"Graph Recipe"}]}""");
        Assert.Equal("Graph Recipe", parser.GetName());
    }

    [Fact]
    public async Task GetName_RecipeInsideJsonArray_ReturnsName()
    {
        var parser = await CreateParser("""[{"@type":"Article"},{"@type":"Recipe","name":"Array Recipe"}]""");
        Assert.Equal("Array Recipe", parser.GetName());
    }

    // --- GetYield ---

    [Fact]
    public async Task GetYield_ReturnsYield()
    {
        var parser = await CreateParser("""{"@type":"Recipe","recipeYield":"4 servings"}""");
        Assert.Equal("4 servings", parser.GetYield());
    }

    [Fact]
    public async Task GetYield_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Null(parser.GetYield());
    }

    // --- GetImage ---

    [Fact]
    public async Task GetImage_AsString_ReturnsImage()
    {
        var parser = await CreateParser("""{"@type":"Recipe","image":"https://example.com/image.jpg"}""");
        Assert.Equal("https://example.com/image.jpg", parser.GetImage());
    }

    [Fact]
    public async Task GetImage_AsArray_ReturnsFirstImage()
    {
        var parser = await CreateParser("""{"@type":"Recipe","image":["https://example.com/1.jpg","https://example.com/2.jpg"]}""");
        Assert.Equal("https://example.com/1.jpg", parser.GetImage());
    }

    [Fact]
    public async Task GetImage_AsObject_ReturnsUrl()
    {
        var parser = await CreateParser("""{"@type":"Recipe","image":{"@type":"ImageObject","url":"https://example.com/image.jpg"}}""");
        Assert.Equal("https://example.com/image.jpg", parser.GetImage());
    }

    [Fact]
    public async Task GetImage_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Null(parser.GetImage());
    }

    // --- GetPrepTime / GetCookTime ---

    [Fact]
    public async Task GetPrepTime_ReturnsTimeSpan()
    {
        var parser = await CreateParser("""{"@type":"Recipe","prepTime":"PT20M"}""");
        Assert.Equal(TimeSpan.FromMinutes(20), parser.GetPrepTime());
    }

    [Fact]
    public async Task GetPrepTime_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Null(parser.GetPrepTime());
    }

    [Fact]
    public async Task GetCookTime_ReturnsTimeSpan()
    {
        var parser = await CreateParser("""{"@type":"Recipe","cookTime":"PT1H30M"}""");
        Assert.Equal(TimeSpan.FromMinutes(90), parser.GetCookTime());
    }

    [Fact]
    public async Task GetCookTime_MissingField_ReturnsNull()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Null(parser.GetCookTime());
    }

    // --- GetRecipeIngredients ---

    [Fact]
    public async Task GetRecipeIngredients_ReturnsIngredients()
    {
        var parser = await CreateParser("""{"@type":"Recipe","recipeIngredient":["2 cups flour","1 tsp salt","3 eggs"]}""");
        var ingredients = parser.GetRecipeIngredients();
        Assert.Equal(3, ingredients.Count);
        Assert.Equal("2 cups flour", ingredients[0]);
        Assert.Equal("1 tsp salt", ingredients[1]);
    }

    [Fact]
    public async Task GetRecipeIngredients_MissingField_ReturnsEmptyList()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Empty(parser.GetRecipeIngredients());
    }

    // --- GetRecipeInstructions ---

    [Fact]
    public async Task GetRecipeInstructions_AsHowToSteps_ReturnsInstructions()
    {
        var parser = await CreateParser("""
            {
                "@type": "Recipe",
                "recipeInstructions": [
                    {"@type":"HowToStep","text":"Preheat oven to 350F."},
                    {"@type":"HowToStep","text":"Mix ingredients together."}
                ]
            }
            """);
        var instructions = parser.GetRecipeInstructions();
        Assert.Equal(2, instructions.Count);
        Assert.Equal("Preheat oven to 350F.", instructions[0]);
        Assert.Equal("Mix ingredients together.", instructions[1]);
    }

    [Fact]
    public async Task GetRecipeInstructions_AsHowToSections_ReturnsAllSteps()
    {
        var parser = await CreateParser("""
            {
                "@type": "Recipe",
                "recipeInstructions": [
                    {"@type":"HowToSection","itemListElement":[
                        {"@type":"HowToStep","text":"Step 1."},
                        {"@type":"HowToStep","text":"Step 2."}
                    ]},
                    {"@type":"HowToSection","itemListElement":[
                        {"@type":"HowToStep","text":"Step 3."}
                    ]}
                ]
            }
            """);
        var instructions = parser.GetRecipeInstructions();
        Assert.Equal(3, instructions.Count);
    }

    [Fact]
    public async Task GetRecipeInstructions_MissingField_ReturnsEmptyList()
    {
        var parser = await CreateParser("""{"@type":"Recipe"}""");
        Assert.Empty(parser.GetRecipeInstructions());
    }
}
