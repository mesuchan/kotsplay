using System.Collections.Generic;

namespace kotsplay.Entities
{
    public class Recipe : BaseIngredient
    {
        public Recipe(int id, string name, List<BaseIngredient> ingredients)
        {
            Id = id;
            Name = name;
            Ingredients = ingredients;
        }

        public List<BaseIngredient> Ingredients { get; set; }
    }
}