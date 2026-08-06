using System;
using MasakanTradisional.Data;

namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Contract for workstation controllers that incorporate interactive player action mechanics
    /// (e.g. Chopping, Mixing, Stirring, Grinding with tools like Knife, Mixer, Mortar & Pestle, Spatula).
    /// </summary>
    public interface IActionStationController : IStationController
    {
        /// <summary>
        /// True if the current step action requires interactive player input (Chop, Mix, Grind, Saute/Stir, etc.).
        /// </summary>
        bool IsActionRequired { get; }

        /// <summary>
        /// True when the player has completed the required action for the current step.
        /// </summary>
        bool IsActionCompleted { get; }

        /// <summary>
        /// Progress of the action from 0.0 (0%) to 1.0 (100%).
        /// </summary>
        float ActionProgress { get; }

        /// <summary>
        /// Calculated quality score (0.0 to 1.0) based on accuracy, rhythm, and completion pace.
        /// </summary>
        float ActionQualityScore { get; }

        /// <summary>
        /// Event fired whenever action progress updates or completes.
        /// </summary>
        event Action OnActionChanged;
    }
}
