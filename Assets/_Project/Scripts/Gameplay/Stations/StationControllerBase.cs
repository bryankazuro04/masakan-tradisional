using UnityEngine;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Base class for all workstation panel controllers. Handles panel visibility and
    /// step caching; concrete stations override RefreshUI() to bind their own widgets.
    /// </summary>
    public abstract class StationControllerBase : MonoBehaviour, IStationController
    {
        [Header("Base Station Setup")]
        [SerializeField] protected GameObject panelRoot;
 
        protected CookingStep currentStep;
 
        public abstract KitchenStationType StationType { get; }
 
        public virtual void Activate(CookingStep step)
        {
            currentStep = step;
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshUI();
        }
 
        public virtual void Deactivate()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
 
        /// <summary>
        /// Override to bind currentStep's data (ingredients, instructions, icons, etc.)
        /// to this station's own UI widgets. Called every time Activate() runs.
        /// </summary>
        protected virtual void RefreshUI() { }
    }
}