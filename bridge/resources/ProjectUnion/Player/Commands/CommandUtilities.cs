using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Commands
{
    public static class CommandUtilities
    {

        public static async Task<bool> VerifyCommandAccess(Client client, string command)
        {
            var playerData = client.GetData(Player.Data.PlayerData.PLAYER_DATA) as PlayerData;

            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(Database.MySQL.connection, playerData.Id, command) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, "You do not have access to that command.");
                return false;
            }
            return true;
        }
    }
}
