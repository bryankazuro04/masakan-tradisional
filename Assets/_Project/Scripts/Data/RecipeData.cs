using System;
using System.Collections.Generic;
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
 
    public enum StepActionType
    {
        Prepare,
        Chop,
        Grind,
        Mix,
        Saute,
        Boil,
        Simmer,
        Fry,
        Bake,
        PlateAndGarnish
    }
 
    [Serializable]
    public struct IngredientData
    {
        public string ingredientId;
        public string ingredientName;
        public Sprite icon;
        public float amount;
        public string unit; // e.g., "g", "ml", "tbsp", "pcs"
        public bool isOptional;
 
        [Tooltip("Which station panel the player must visit to collect this ingredient during Kitchen Prep.")]
        public KitchenStationType sourceStation;
    }
 
    [Serializable]
    public struct ToolData
    {
        public string toolId;
        public string toolName;
        public Sprite icon;
        public string toolType; // e.g., "Mortar & Pestle", "Wok", "Knife"
 
        [Tooltip("Which station panel the player must visit to collect this tool during Kitchen Prep.")]
        public KitchenStationType sourceStation;
    }
 
    [Serializable]
    public class CookingStep
    {
        [Header("Step Overview")]
        public string stepId;
        public string stepTitle;
        [TextArea(2, 4)]
        public string instruction;
        public StepActionType actionType;
 
        [Header("Requirements")]
        public ToolData requiredTool;
        public List<IngredientData> requiredIngredients = new List<IngredientData>();
 
        [Header("Execution Parameters")]
        public float timeLimitSeconds = 30f;
        public float targetTemperature; // Applicable for cooking steps
        public Sprite stepIllustration;
        public AudioClip stepSFX;
 
        [Header("Fuzzy Logic Overrides (Optional)")]
        [Tooltip("Optional step-specific Fuzzy Logic override asset. Overrides recipe default if assigned.")]
        public MasakanTradisional.Gameplay.Evaluation.FuzzyCookingData fuzzyData;
 
        [Header("Scoring Calibration")]
        [Range(0.5f, 2.0f)]
        public float scoreMultiplier = 1.0f;
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
        public string gameplaySceneName = "Gameplay";      // e.g., "Cooking_Rendang"
        public Sprite recipeIcon;             // Thumbnail icon for UI card
        public Sprite dishBannerImage;        // High-res preview image
 
        [Header("Fuzzy Logic Configuration")]
        [Tooltip("Recipe-wide Fuzzy Logic configuration asset.")]
        [SerializeField] private MasakanTradisional.Gameplay.Evaluation.FuzzyCookingData defaultFuzzyData;
 
        [Header("Unlock Requirements")]
        public int starsRequiredToUnlock = 0; // Cumulative stars needed
        public RecipeData prerequisiteRecipe; // Optional: recipe required prior to this
 
        [Header("Cooking Specifications")]
        [SerializeField] private List<IngredientData> ingredients = new List<IngredientData>();
        [SerializeField] private List<ToolData> requiredTools = new List<ToolData>();
        [SerializeField] private List<CookingStep> stepSequence = new List<CookingStep>();
 
        // Public Accessors
        public MasakanTradisional.Gameplay.Evaluation.FuzzyCookingData DefaultFuzzyData => defaultFuzzyData;
        public IReadOnlyList<IngredientData> Ingredients => ingredients;
        public IReadOnlyList<ToolData> RequiredTools => requiredTools;
        public IReadOnlyList<CookingStep> StepSequence => stepSequence;
 
        public int StepCount => stepSequence.Count;
 
        public CookingStep GetStep(int index)
        {
            if (index >= 0 && index < stepSequence.Count)
            {
                return stepSequence[index];
            }
            Debug.LogWarning($"[RecipeData] Invalid step index {index} requested for recipe '{recipeName}'.");
            return null;
        }
    }
}