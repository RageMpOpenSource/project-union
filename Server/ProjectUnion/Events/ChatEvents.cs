using GTANetworkAPI;
using ProjectUnion.Data;
using System;

namespace ProjectUnion.Server
{
    public class ChatEvents : Script
    {

        [ServerEvent(Event.ChatMessage)]
        private void OnChatMessage(Client client, string message)
        {
            ServerUtilities.SetPlayerNametag(client);
        }
    }
}
