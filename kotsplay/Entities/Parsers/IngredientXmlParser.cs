using System;
using System.Xml;

namespace kotsplay.Entities.Parsers
{
    public class IngredientXmlParser
    {
        public const string IngredientTag = "Ingredient";
        public const string NameAttribute = "name";
        public const string IdAttribute = "id";

        public Ingredient Parse(XmlReader reader)
        {
            if (reader.Name != IngredientTag || !reader.IsStartElement())
                throw new ParseException();
            try
            {
                var id = int.Parse(reader.GetAttribute(IdAttribute));
                var name = reader.GetAttribute(NameAttribute);
                return new Ingredient(id, name);
            }
            catch (FormatException)
            {
                throw new ParseException();
            }
        }
    }
}