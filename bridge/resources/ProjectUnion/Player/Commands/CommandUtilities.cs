using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Commands
{
    public static class CommandUtilities
    {

        public static async Task<bool> VerifyCommandAccess(Client client, string command)
        {
            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_ID);
            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(Database.MySQL.connection, playerData.Id, command) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, "You do not have access to that command.");
                return false;
            }
            return true;
        }

        public static async void WriteHelpResponse(Client client, string title, List<string> commands)
        {
            var response = "";

            var totalCommands = 0;
            for (int i = 0; i < commands.Count; i++)
            {
                string command = commands[i];

                if (await VerifyCommandAccess(client, command) == false) continue;
                totalCommands++;
                response += "/" + command;

                if (i < commands.Count - 1)
                {
                    response += ", ";
                }
            }

            if (totalCommands == 0)
            {
                response += "You do not have access to any of these commands.";
            }

            NAPI.Chat.SendChatMessageToPlayer(client, title);
            NAPI.Chat.SendChatMessageToPlayer(client, response);
        }
    }
}
