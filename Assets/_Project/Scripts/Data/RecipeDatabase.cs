using System.Collections.Generic;
using UnityEngine;

namespace MasakanTradisional.Data
{
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Masakan Tradisional/Data/Recipe Database")]
    public class RecipeDatabase : ScriptableObject
    {
        [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();

        public IReadOnlyList<RecipeData> Recipes => recipes;

        public RecipeData GetRecipeById(string id)
        {
            return recipes.Find(r => r != null && r.recipeId == id);
        }

        public int TotalRecipeCount => recipes.Count;
    }
}
