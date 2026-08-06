using System;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Contract for stations where the player must click to collect required
    /// ingredients/tools before a step can be finished (Shelves, Fridge, Counter
    /// today; any future station showing pickable items can implement this too).
    /// </summary>
    public interface ICollectibleStation : IStationController
    {
        /// <summary>True once every item this station is responsible for (for the current step) has been collected. Vacuously true if the step has no items assigned to this station.</summary>
        bool AllRequiredItemsCollected { get; }
 
        /// <summary>Raised whenever collection progress changes, so GameplayManager can re-check whether Finish Step should be enabled.</summary>
        event Action OnCollectionChanged;
    }
}