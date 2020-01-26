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

        [Command("dc")]
        public void CMD_Disconnect(Client client)
        {
            NAPI.Player.KickPlayer(client, "reconnect");
        }


    }
}
