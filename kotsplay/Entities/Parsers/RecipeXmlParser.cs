using System;
using System.Collections.Generic;
using System.Xml;

namespace kotsplay.Entities.Parsers
{
    public class RecipeXmlParser
    {
        public const string RecipeTag = "Recipe";
        public const string NameAttribute = "name";
        public const string IdAttribute = "id";
        public const string IngredientIdTag = "IngredientId";

        public Recipe Parse(XmlReader reader, Dictionary<int, BaseIngredient> baseIngredients)
        {
            if (reader.Name != RecipeTag || !reader.IsStartElement())
                throw new ParseException();
            try
            {
                var id = int.Parse(reader.GetAttribute(IdAttribute));
                var name = reader.GetAttribute(NameAttribute);
                var ingredients = new List<BaseIngredient>();
                while (reader.Read() && reader.Name != RecipeTag)
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == IngredientIdTag)
                        {
                            var ingredientId = reader.ReadElementContentAsInt();
                            if (!baseIngredients.ContainsKey(ingredientId))
                                throw new ParseException();
                            ingredients.Add(baseIngredients[ingredientId]);
                        }
                        else
                        {
                            throw new ParseException();
                        }
                    }
                }
                return new Recipe(id, name, ingredients);
            }
            catch (FormatException)
            {
                throw new ParseException();
            }
        }
    }
}