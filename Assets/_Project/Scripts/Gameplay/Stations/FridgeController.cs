using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Fridge workstation: cold/perishable ingredients, used for Prepare steps.
    /// </summary>
    public class FridgeController : IngredientDisplayStationBase
    {
        public override KitchenStationType StationType => KitchenStationType.Fridge;
    }
}