using System.Text;
using TMPro;
using UnityEngine;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Shared base for simple stations that just need to display the current step's
    /// required ingredients (Shelves, Fridge, Counter). Extend a concrete subclass
    /// with drag-and-drop or interaction logic later without touching this base.
    /// </summary>
    public abstract class IngredientDisplayStationBase : StationControllerBase
    {
        [Header("Ingredient Display")]
        [SerializeField] protected TMP_Text ingredientListText;
 
        protected override void RefreshUI()
        {
            if (ingredientListText == null || currentStep == null) return;
            ingredientListText.text = BuildIngredientListText();
        }
 
        private string BuildIngredientListText()
        {
            if (currentStep.requiredIngredients == null || currentStep.requiredIngredients.Count == 0)
                return "No ingredients required for this step.";
 
            var lines = new StringBuilder();
            foreach (var ing in currentStep.requiredIngredients)
            {
                lines.AppendLine($"- {ing.ingredientName} ({ing.amount} {ing.unit})");
            }
            return lines.ToString();
        }
    }
}