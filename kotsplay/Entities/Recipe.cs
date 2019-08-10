using System.Collections.Generic;

namespace kotsplay.Entities
{
    public class Recipe : BaseIngredient
    {
        public List<BaseIngredient> Ingridients { get; set; }
    }
}