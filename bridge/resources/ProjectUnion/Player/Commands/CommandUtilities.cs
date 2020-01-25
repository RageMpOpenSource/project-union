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
            //TODO : Player Data
            var playerData = client.GetData(Player.Data.PlayerData.PLAYER_DATA) as PlayerData;

            //TEMP TILL PLAYER DATA IMPLEMENTED
            playerData = new PlayerData()
            {
                Id = 0
            };

            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(Database.MySQL.connection, playerData.Id, command) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, "You do not have access to that command.");
                return false;
            }
            return true;
        }

        public static async void WriteHelpResponse(Client client, string title, List<string> commands)
        {
            var response = title;

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

            NAPI.Chat.SendChatMessageToPlayer(client, response);
            NAPI.Util.ConsoleOutput($"{response}");
        }
    }
}
