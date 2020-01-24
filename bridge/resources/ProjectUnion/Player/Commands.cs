using GTANetworkAPI;

namespace ProjectUnion.Player
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


        [Command("pos")]
        public void CMD_GetPosition(Client client)
        {
            NAPI.Chat.SendChatMessageToPlayer(client, $"Position {client.Position} | {client.Heading}");
        }


        [Command("veh")]
        public void CMD_SpawnVeh(Client client, string vehName)
        {
            NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehName), client.Position, client.Heading, 112, 112);
        }
    }
}
