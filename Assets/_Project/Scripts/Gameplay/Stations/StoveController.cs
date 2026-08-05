namespace MasakanTradisional.Gameplay.Stations
{
    /// <summary>
    /// Stove workstation: Saute / Boil / Simmer / Fry steps.
    /// All behaviour is inherited from HeatStationControllerBase - this class only
    /// identifies which station type it represents.
    /// </summary>
    public class StoveController : HeatStationControllerBase
    {
        public override KitchenStationType StationType => KitchenStationType.Stove;
    }
}