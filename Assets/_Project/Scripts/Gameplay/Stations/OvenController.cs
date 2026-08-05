using UnityEngine;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Oven workstation: Bake steps. Reuses all heat/timer logic from
    /// HeatStationControllerBase for free. Oven-specific extras (door animation,
    /// preheat delay, etc.) can be added here without touching Stove or the base class.
    /// </summary>
    public class OvenController : HeatStationControllerBase
    {
        public override KitchenStationType StationType => KitchenStationType.Oven;
 
        [Header("Oven Extras (optional)")]
        [SerializeField] private Animator ovenDoorAnimator;
 
        public override void Activate(CookingStep step)
        {
            base.Activate(step);
            if (ovenDoorAnimator != null) ovenDoorAnimator.SetBool("IsOpen", true);
        }
 
        public override void Deactivate()
        {
            base.Deactivate();
            if (ovenDoorAnimator != null) ovenDoorAnimator.SetBool("IsOpen", false);
        }
    }
}