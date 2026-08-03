using UnityEngine;

namespace MasakanTradisional.Data
{
    public enum RecipeDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    [CreateAssetMenu(fileName = "NewRecipeData", menuName = "Masakan Tradisional/Data/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        [Header("Basic Information")]
        public string recipeId;               // e.g., "recipe_rendang"
        public string recipeName;             // e.g., "Rendang Padang"
        public string originRegion;           // e.g., "Padang, West Sumatra"
        [TextArea(3, 5)] 
        public string description;            // Cultural & culinary history

        [Header("Gameplay Configuration")]
        public RecipeDifficulty difficulty = RecipeDifficulty.Medium;
        public string gameplaySceneName;      // e.g., "Cooking_Rendang"
        public Sprite recipeIcon;             // Thumbnail icon for UI card
        public Sprite dishBannerImage;        // High-res preview image

        [Header("Unlock Requirements")]
        public int starsRequiredToUnlock = 0; // Cumulative stars needed
        public RecipeData prerequisiteRecipe; // Optional: recipe required prior to this
    }
}