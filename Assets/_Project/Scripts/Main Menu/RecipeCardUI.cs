using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MasakanTradisional.Data;

namespace MasakanTradisional.UI.MainMenu
{
    public class RecipeCardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text recipeTitleText;
        [SerializeField] private TMP_Text regionText;
        [SerializeField] private Image recipeImage;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;

        [Header("Extended Recipe Details")]
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text stepCountText;
        [SerializeField] private TMP_Text ingredientCountText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Lock / Unlock UI")]
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private TMP_Text lockRequirementText;
        [SerializeField] private Button selectButton;

        public void SetupCard(RecipeData recipeData, int earnedStars, bool isUnlocked, System.Action<RecipeData> onRecipeSelected)
        {
            if (recipeTitleText != null) recipeTitleText.text = recipeData.recipeName;
            if (regionText != null) regionText.text = recipeData.originRegion;
            if (recipeImage != null) recipeImage.sprite = recipeData.recipeIcon;

            // Render extended details
            if (difficultyText != null) difficultyText.text = recipeData.difficulty.ToString();
            if (stepCountText != null) stepCountText.text = $"{recipeData.StepCount} Steps";
            if (ingredientCountText != null) ingredientCountText.text = $"{recipeData.Ingredients.Count} Ingredients";
            if (descriptionText != null) descriptionText.text = recipeData.description;

            // Render Stars Rating
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = (i < earnedStars) ? filledStarSprite : emptyStarSprite;
                }
            }

            // Handle Lock Overlay
            if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
            if (lockRequirementText != null && !isUnlocked)
            {
                lockRequirementText.text = $"Requires {recipeData.starsRequiredToUnlock} ★";
            }

            // Select Button Configuration
            if (selectButton != null)
            {
                selectButton.interactable = isUnlocked;
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => {
                    // Notify state machine directly of recipe selection FIRST before loading scene
                    MasakanTradisional.Core.FSM.GameStateMachine.SetSelectedRecipe(recipeData);
                    onRecipeSelected?.Invoke(recipeData);
                });
            }
        }
    }
}