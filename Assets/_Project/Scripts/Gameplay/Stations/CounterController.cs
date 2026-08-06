using MasakanTradisional.Data;
 
namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Counter workstation: dry goods / spices, ingredients, and prep work.
    /// </summary>
    public class CounterController : IngredientDisplayStationBase
    {
        public override KitchenStationType StationType => KitchenStationType.Counter;
    }
}