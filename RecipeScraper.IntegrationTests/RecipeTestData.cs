using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RecipeScraper.IntegrationTests;

public static class RecipeTestData
{
    public static readonly RecipeTestCase[] All = JsonSerializer.Deserialize<RecipeTestCase[]>(File.ReadAllText("RecipeTestData.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    public static IEnumerable<object[]> Sites => All.Select(tc => new object[] { tc });
}
