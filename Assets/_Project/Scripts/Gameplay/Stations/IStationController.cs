using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Contract for any workstation panel (Shelves, Fridge, Counter, Stove, Oven, ...).
    /// GameplayManager talks to stations only through this interface, never through a
    /// concrete type - that's what lets you add a new station without editing GameplayManager.
    /// </summary>
    public interface IStationController
    {
        KitchenStationType StationType { get; }
 
        /// <summary>
        /// Called when this station's panel becomes the visible/active one.
        /// Should refresh all UI to reflect the given step.
        /// </summary>
        void Activate(CookingStep step);
 
        /// <summary>
        /// Called when this station's panel is hidden (player navigated to another station).
        /// </summary>
        void Deactivate();
    }
 
    /// <summary>
    /// Extended contract for stations that track a manipulable heat value and an elapsed
    /// cooking timer (Stove, Oven, and any future heat-based station).
    /// </summary>
    public interface IHeatStationController : IStationController
    {
        float CurrentHeat { get; }
        float ElapsedCookingTime { get; }
    }
}