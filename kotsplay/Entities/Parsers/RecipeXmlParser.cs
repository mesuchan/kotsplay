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
        public const string IngridientIdTag = "IngridientId";

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
                    if (reader.Name == IngridientIdTag)
                    {
                        var ingredientId = reader.ReadContentAsInt();
                        if (!baseIngredients.ContainsKey(ingredientId))
                            throw new ParseException();
                        ingredients.Add(baseIngredients[ingredientId]);
                        reader.ReadEndElement();
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
                if (reader.Name != RecipeTag)  // closing tag
                    throw new XmlException();
                return new Recipe(id, name, ingredients);
            }
            catch (FormatException)
            {
                throw new ParseException();
            }
        }
    }
}