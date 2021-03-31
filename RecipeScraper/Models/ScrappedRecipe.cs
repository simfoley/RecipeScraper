using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeScraper.Models
{
    public class ScrappedRecipe
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Yield { get; set; }
        public TimeSpan PrepTime { get; set; }
        public TimeSpan CookTime { get; set; }

        public string[] RecipeIngredients { get; set; }
        public string[] RecipeInstructions { get; set; }
    }
}
