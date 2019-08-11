using System.Collections.Generic;
using System.Xml;

namespace kotsplay.Entities.Parsers
{
    public class BookXmlParser
    {
        public const string BookTag = "Book";
        public const string NameAttribute = "name";

        public BookXmlParser(IngredientXmlParser ingredientXmlParser, RecipeXmlParser recipeXmlParser)
        {
            this.ingredientXmlParser = ingredientXmlParser;
            this.recipeXmlParser = recipeXmlParser;
        }

        public Book Parse(XmlReader reader)
        {
            if (reader.Name != BookTag || !reader.IsStartElement())
                throw new ParseException();
            var name = reader.GetAttribute(NameAttribute);
            var ingredients = new List<Ingredient>();
            var recipes = new List<Recipe>();
            var baseIngredients = new Dictionary<int, BaseIngredient>();
            while (reader.Read() && reader.Name != BookTag)
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case IngredientXmlParser.IngredientTag:
                            var ingredient = ingredientXmlParser.Parse(reader);
                            ingredients.Add(ingredient);
                            baseIngredients.Add(ingredient.Id, ingredient);
                            break;
                        case RecipeXmlParser.RecipeTag:
                            var recipe = recipeXmlParser.Parse(reader, baseIngredients);
                            recipes.Add(recipe);
                            baseIngredients.Add(recipe.Id, recipe);
                            break;
                        default:
                            throw new ParseException();
                    }
                }
            }
            if (reader.Name != BookTag)  // closing tag
                throw new XmlException();
            return new Book(name, ingredients, recipes);
        }

        protected readonly IngredientXmlParser ingredientXmlParser;
        protected readonly RecipeXmlParser recipeXmlParser;
    }
}