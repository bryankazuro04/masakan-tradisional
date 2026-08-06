using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Shelves workstation: dry goods / spices, used mainly for Prepare/Grind steps.
    /// </summary>
    public class ShelvesController : IngredientDisplayStationBase
    {
        public override KitchenStationType StationType => KitchenStationType.Shelves;
    }
}