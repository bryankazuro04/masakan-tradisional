using System;
using System.Collections.Generic;
using UnityEngine;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Shared base for stations where the player collects required ingredients/tools
    /// by clicking them (Shelves, Fridge, Counter). Only items whose sourceStation
    /// matches this station's StationType are spawned here - the same step's other
    /// items appear on their own station panels. Runs its refresh in SetStep, so
    /// hidden panels stay correctly in sync even while another station is visible.
    /// </summary>
    public abstract class IngredientDisplayStationBase : StationControllerBase, ICollectibleStation
    {
        [Header("Collectible Item Display")]
        [SerializeField] protected Transform itemContainer;
        [SerializeField] protected CollectibleItemView itemViewPrefab;
 
        private readonly List<CollectibleItemView> spawnedViews = new List<CollectibleItemView>();
        private int totalRequiredCount;
        private int collectedCount;
 
        public bool AllRequiredItemsCollected => collectedCount >= totalRequiredCount;
        public event Action OnCollectionChanged;
 
        protected override void RefreshUI()
        {
            ClearSpawnedViews();
            collectedCount = 0;
            totalRequiredCount = 0;
 
            if (currentStep != null && itemContainer != null && itemViewPrefab != null)
            {
                foreach (var ingredient in currentStep.requiredIngredients)
                {
                    if (ingredient.sourceStation != StationType) continue;
                    SpawnItem(ingredient.icon, ingredient.ingredientName);
                }
 
                if (!string.IsNullOrEmpty(currentStep.requiredTool.toolId) &&
                    currentStep.requiredTool.sourceStation == StationType)
                {
                    SpawnItem(currentStep.requiredTool.icon, currentStep.requiredTool.toolName);
                }
            }
 
            // Notifying even with zero items matters: it's how GameplayManager learns
            // this station is vacuously satisfied for a step it has nothing to do with.
            OnCollectionChanged?.Invoke();
        }
 
        private void SpawnItem(Sprite icon, string displayName)
        {
            totalRequiredCount++;
            CollectibleItemView view = Instantiate(itemViewPrefab, itemContainer);
            view.Setup(icon, displayName, HandleItemCollected);
            spawnedViews.Add(view);
        }
 
        private void HandleItemCollected()
        {
            collectedCount++;
            OnCollectionChanged?.Invoke();
        }
 
        private void ClearSpawnedViews()
        {
            foreach (var view in spawnedViews)
            {
                if (view != null) Destroy(view.gameObject);
            }
            spawnedViews.Clear();
        }
    }
}