using UnityEngine;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Base class for all workstation panel controllers. Handles the SetStep/Show/Hide
    /// split; concrete stations override RefreshUI() to bind their own widgets.
    /// </summary>
    public abstract class StationControllerBase : MonoBehaviour, IStationController
    {
        [Header("Base Station Setup")]
        [SerializeField] protected GameObject panelRoot;
 
        protected CookingStep currentStep;
 
        public abstract KitchenStationType StationType { get; }
 
        public virtual void SetStep(CookingStep step)
        {
            currentStep = step;
            RefreshUI();
        }
 
        public virtual void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
        }
 
        public virtual void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
 
        /// <summary>
        /// Override to bind currentStep's data (ingredients, instructions, icons, etc.)
        /// to this station's own UI widgets. Called every time SetStep() runs -
        /// i.e. once per step, not once per time the panel is shown.
        /// </summary>
        protected virtual void RefreshUI() { }
    }
}