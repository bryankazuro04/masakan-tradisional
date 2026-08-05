using UnityEngine;
using MasakanTradisional.Data;

namespace MasakanTradisional.UI.MainMenu
{
    public class RecipeSelectUI : MonoBehaviour
    {
        [Header("Data References")]
        [SerializeField] private RecipeDatabase recipeDatabase;
        [SerializeField] private GameObject recipeCardPrefab;
        [SerializeField] private Transform cardContainerParent;

        [Header("Main Manager")]
        [SerializeField] private MainMenuManager mainMenuManager;

        private void OnEnable()
        {
            PopulateRecipeGrid();
        }

        public void PopulateRecipeGrid()
        {
            if (cardContainerParent == null) return;

            // Clear existing UI cards
            foreach (Transform child in cardContainerParent)
            {
                Destroy(child.gameObject);
            }

            if (recipeDatabase == null || recipeDatabase.Recipes == null)
            {
                Debug.LogWarning("[RecipeSelectUI] RecipeDatabase is null or empty!");
                return;
            }

            var recipes = recipeDatabase.Recipes;
            int totalStarsEarned = CalculateTotalStarsEarned();

            for (int i = 0; i < recipes.Count; i++)
            {
                RecipeData recipe = recipes[i];
                if (recipe == null) continue;

                GameObject cardObj = Instantiate(recipeCardPrefab, cardContainerParent);
                RecipeCardUI cardUI = cardObj.GetComponent<RecipeCardUI>();

                // Get stars scored for this specific recipe
                int stars = PlayerPrefs.GetInt($"Recipe_Stars_{recipe.recipeId}", 0);

                // Check unlock requirements (cumulative stars & prerequisite recipe)
                bool isUnlocked = CheckIsUnlocked(recipe, totalStarsEarned);

                if (cardUI != null)
                {
                    cardUI.SetupCard(recipe, stars, isUnlocked, OnRecipeSelected);
                }
            }
        }

        private bool CheckIsUnlocked(RecipeData recipe, int totalStarsEarned)
        {
            // Cumulative star check
            if (totalStarsEarned < recipe.starsRequiredToUnlock)
                return false;

            // Prerequisite recipe check
            if (recipe.prerequisiteRecipe != null)
            {
                int prereqStars = PlayerPrefs.GetInt($"Recipe_Stars_{recipe.prerequisiteRecipe.recipeId}", 0);
                if (prereqStars <= 0) return false;
            }

            return true;
        }

        private int CalculateTotalStarsEarned()
        {
            int total = 0;
            if (recipeDatabase != null && recipeDatabase.Recipes != null)
            {
                foreach (var recipe in recipeDatabase.Recipes)
                {
                    if (recipe != null)
                    {
                        total += PlayerPrefs.GetInt($"Recipe_Stars_{recipe.recipeId}", 0);
                    }
                }
            }
            return total;
        }

        private void OnRecipeSelected(string sceneName)
        {
            if (mainMenuManager != null)
            {
                // Enforce single-scene architecture: Always load "Gameplay" scene.
                // The selected recipe object is already passed to GameStateMachine.Instance.
                mainMenuManager.LoadGameplayScene("Gameplay");
            }
        }
    }
}