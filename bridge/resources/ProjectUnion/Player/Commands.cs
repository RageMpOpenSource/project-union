using GTANetworkAPI;
using System;

namespace ProjectUnionFreeroam.Player
{
    public class Commands : Script
    {

        [Command("freeze")]
        public void CMD_FreezeTarget(Client client, string targetName)
        {
            var target = NAPI.Player.GetPlayerFromName(targetName);
            NAPI.ClientEvent.TriggerClientEvent(target, "FreezePlayer", true, client.Name);
        }

        [Command("unfreeze")]
        public void CMD_UnFreezeTarget(Client client, string targetName)
        {
            var target = NAPI.Player.GetPlayerFromName(targetName);
            NAPI.ClientEvent.TriggerClientEvent(target, "FreezePlayer", false, client.Name);
        }
    }
}
