using RAGE;
using RAGE.Elements;
using System;

namespace ClientGameModes
{
    public class GameModeEvents : Events.Script
    {
        public GameModeEvents()
        {
            Events.Add("TriggerVehicleExplosion", BlowUpVehicle);
        }

        private void BlowUpVehicle(object[] args)
        {
            int vehicle = RAGE.Game.Player.GetPlayersLastVehicle();
            RAGE.Game.Vehicle.ExplodeVehicle(vehicle, true, false);
        }
    }
}
