using System;
using UnityEngine;
using MasakanTradisional.Data;

namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Stove workstation: Saute / Boil / Simmer / Fry steps.
    /// Inherits heat & timer controls from HeatStationControllerBase and implements IActionStationController for stirring actions.
    /// </summary>
    public class StoveController : HeatStationControllerBase, IActionStationController
    {
        [Header("Stirring / Action Mechanic Integration")]
        [SerializeField] private ActionMechanicController actionController;

        public override KitchenStationType StationType => KitchenStationType.Stove;

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

        protected override void Awake()
        {
            base.Awake();
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

                actionController.OnActionProgressUpdated += HandleActionChanged;
                actionController.OnActionCompleted += HandleActionChanged;
            }
        }

        private void OnDestroy()
        {
            if (actionController != null)
            {
                actionController.OnActionProgressUpdated -= HandleActionChanged;
                actionController.OnActionCompleted -= HandleActionChanged;
            }
        }

        public override void SetStep(CookingStep step)
        {
            base.SetStep(step);

            EnsureActionController();
            if (actionController != null)
            {
                actionController.SetStep(step);
                if (IsActionRequired)
                {
                    actionController.Show();
                }
                else
                {
                    actionController.Hide();
                }
            }
        }

        private void HandleActionChanged()
        {
            OnActionChanged?.Invoke();
        }
    }
}