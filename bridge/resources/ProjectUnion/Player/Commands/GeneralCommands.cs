using GTANetworkAPI;

namespace ProjectUnion.Player.Commands
{
    public class GeneralCommands : Script
    {
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
