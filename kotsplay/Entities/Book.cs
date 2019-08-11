using kotsplay.Entities.Parsers;
using System.Collections.Generic;
using System.Xml;

namespace kotsplay.Entities
{
    public class Book
    {
        public List<Recipe> Recipes { get; private set; }

        public Book(string name, List<Ingredient> ingredients, List<Recipe> recipes)
        {
            Recipes = recipes;
            foreach (var ingredient in ingredients)
            {
                if (!this.ingredients.ContainsKey(ingredient.Id))
                    this.ingredients.Add(ingredient.Id, ingredient);
            }
            foreach (var recipe in recipes)
            {
                if (!this.ingredients.ContainsKey(recipe.Id))
                    this.ingredients.Add(recipe.Id, recipe);
            }
        }

        public BaseIngredient GetIngredient(int id)
        {
            return ingredients.GetValueOrDefault(id);
        }

        public static Book LoadFromXml(string path)
        {
            var parser = new BookXmlParser(new IngredientXmlParser(), new RecipeXmlParser());
            var reader = XmlReader.Create(path);
            return parser.Parse(reader);
        }

        private readonly Dictionary<int, BaseIngredient> ingredients = new Dictionary<int, BaseIngredient>();
    }
}