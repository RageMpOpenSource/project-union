using GTANetworkAPI;
using System.Collections.Generic;

namespace ProjectUnion.Player.Commands
{
    public class AdminCommands : Script, ICommands
    {

        public static readonly List<string> AllAdminCommands = new List<string> { COMMAND_FREEZE, COMMAND_UNFREEZE, COMMAND_KICK, COMMAND_INVISIBLE, COMMAND_VISIBLE, COMMAND_TELEPORT };

        public const string COMMAND_FREEZE = "freeze";
        public const string COMMAND_UNFREEZE = "unfreeze";
        public const string COMMAND_KICK = "kick";
        public const string COMMAND_INVISIBLE = "invisible";
        public const string COMMAND_VISIBLE = "visible";
        public const string COMMAND_TELEPORT = "tp";


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

        [Command("helpadmin")]
        public void CMD_HelpAdminResponse(Client client)
        {
            var title = "";
            var commands = new List<string>();
            GetCommandHelpResponse(out title, out commands);
            CommandUtilities.WriteHelpResponse(client, title, commands);
        }

        public List<string> GetAllCommands()
        {
            return AllAdminCommands;
        }

        public void GetCommandHelpResponse(out string title, out List<string> commands)
        {
            title = "Admin Commands Help: \n";
            commands = AllAdminCommands;
        }
    }
}
