using System.Collections.Generic;

namespace kotsplay.Entities
{
    public class Book
    {
        public List<Recipe> Recipes { get; private set; }

        public Book(string name, List<Ingridient> ingridients, List<Recipe> recipes)
        {
            Recipes = recipes;
            foreach (var ingridient in ingridients)
            {
                if (!this.ingridients.ContainsKey(ingridient.Id))
                    this.ingridients.Add(ingridient.Id, ingridient);
            }
            foreach (var recipe in recipes)
            {
                if (!this.ingridients.ContainsKey(recipe.Id))
                    this.ingridients.Add(recipe.Id, recipe);
            }
        }

        public BaseIngridient GetIngridient(int id)
        {
            return ingridients.GetValueOrDefault(id);
        }

        private readonly Dictionary<int, BaseIngridient> ingridients = new Dictionary<int, BaseIngridient>();
    }
}