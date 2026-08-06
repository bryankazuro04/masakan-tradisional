using UnityEngine;
using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Oven workstation: Bake steps. Reuses all heat/timer logic from
    /// HeatStationControllerBase. Door animation now hooks into Show/Hide (pure
    /// visibility) rather than SetStep, so it only reacts to the player actually
    /// looking at the oven, not to step changes happening elsewhere.
    /// </summary>
    public class OvenController : HeatStationControllerBase
    {
        public override KitchenStationType StationType => KitchenStationType.Oven;
 
        [Header("Oven Extras (optional)")]
        [SerializeField] private Animator ovenDoorAnimator;
 
        public override void Show()
        {
            base.Show();
            if (ovenDoorAnimator != null) ovenDoorAnimator.SetBool("IsOpen", true);
        }
 
        public override void Hide()
        {
            base.Hide();
            if (ovenDoorAnimator != null) ovenDoorAnimator.SetBool("IsOpen", false);
        }
    }
}