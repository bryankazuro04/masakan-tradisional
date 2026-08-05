namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Counter workstation: Chop / Grind / Mix / PlateAndGarnish steps.
    /// </summary>
    public class CounterController : IngredientDisplayStationBase
    {
        public override KitchenStationType StationType => KitchenStationType.Counter;
    }
}