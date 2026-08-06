using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Contract for any workstation panel (Shelves, Fridge, Counter, Stove, Oven, ...).
    /// GameplayManager talks to stations only through this interface.
    ///
    /// SetStep and Show/Hide are intentionally separate: SetStep is called on EVERY
    /// registered station whenever the current recipe step changes, even if that
    /// station's panel isn't currently visible. Show/Hide only toggle visibility.
    /// This keeps hidden panels in sync (e.g. collection state) and stops browsing
    /// between stations from resetting a station's in-progress state (e.g. a
    /// stove's heat/timer) just because the player looked away and came back.
    /// </summary>
    public interface IStationController
    {
        KitchenStationType StationType { get; }
 
        /// <summary>Sync this station's internal state to the given step. Called for every station on every step change, regardless of visibility.</summary>
        void SetStep(CookingStep step);
 
        /// <summary>Make this station's panel visible.</summary>
        void Show();
 
        /// <summary>Hide this station's panel.</summary>
        void Hide();
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