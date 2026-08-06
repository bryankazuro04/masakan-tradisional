using System;
using UnityEngine;
using MasakanTradisional.Data;

namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Counter workstation: dry goods / spices, ingredients, and prep action work (Chopping, Mixing, Grinding).
    /// Inherits item collection from IngredientDisplayStationBase and implements IActionStationController.
    /// </summary>
    public class CounterController : IngredientDisplayStationBase, IActionStationController
    {
        [Header("Action Mechanic Integration")]
        [SerializeField] private ActionMechanicController actionController;

        public override KitchenStationType StationType => KitchenStationType.Counter;

        // IActionStationController Implementation
        public bool IsActionRequired
        {
            get
            {
                EnsureActionController();
                return actionController != null && actionController.IsActionRequired;
            }
        }

        public bool IsActionCompleted
        {
            get
            {
                EnsureActionController();
                return actionController == null || !actionController.IsActionRequired || actionController.IsActionCompleted;
            }
        }

        public float ActionProgress => actionController != null ? actionController.ActionProgress : 1.0f;
        public float ActionQualityScore => actionController != null ? actionController.ActionQualityScore : 1.0f;

        public event Action OnActionChanged;

        private void Awake()
        {
            EnsureActionController();
        }

        private void EnsureActionController()
        {
            if (actionController == null)
            {
                actionController = GetComponentInChildren<ActionMechanicController>();
            }

            if (actionController == null)
            {
                Transform parentTransform = panelRoot != null ? panelRoot.transform : transform;
                GameObject actionObj = new GameObject("ActionMechanicOverlay", typeof(RectTransform));
                actionObj.transform.SetParent(parentTransform, false);
                actionController = actionObj.AddComponent<ActionMechanicController>();
                actionController.EnsureRuntimeUI();

                actionController.OnActionProgressUpdated += HandleActionProgressUpdated;
                actionController.OnActionCompleted += HandleActionCompleted;
            }
        }

        private void OnDestroy()
        {
            if (actionController != null)
            {
                actionController.OnActionProgressUpdated -= HandleActionProgressUpdated;
                actionController.OnActionCompleted -= HandleActionCompleted;
            }
        }

        public override void SetStep(CookingStep step)
        {
            base.SetStep(step); // Refreshes item collection display

            EnsureActionController();
            if (actionController != null)
            {
                actionController.SetStep(step);
                UpdateActionVisibility();
            }
        }

        private void HandleActionProgressUpdated()
        {
            OnActionChanged?.Invoke();
        }

        private void HandleActionCompleted()
        {
            OnActionChanged?.Invoke();
        }

        private void UpdateActionVisibility()
        {
            if (actionController == null) return;

            // Show action UI once all required items for the step are collected
            if (IsActionRequired && AllRequiredItemsCollected)
            {
                actionController.Show();
            }
            else
            {
                actionController.Hide();
            }
        }

        protected override void RefreshUI()
        {
            base.RefreshUI();
            UpdateActionVisibility();
        }
    }
}