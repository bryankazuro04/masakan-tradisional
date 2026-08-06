using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Shelves workstation: equipment / tools storage, used mainly for tool collection.
    /// </summary>
    public class ShelvesController : IngredientDisplayStationBase
    {
        public override KitchenStationType StationType => KitchenStationType.Shelves;
    }
}