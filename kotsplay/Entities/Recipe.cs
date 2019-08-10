using System.Collections.Generic;

namespace kotsplay.Entities
{
    public class Recipe : BaseIngridient
    {
        public List<BaseIngridient> Ingridients { get; set; }
    }
}