using GTANetworkAPI;
using System.Collections.Generic;

namespace ProjectUnion.Player.Commands
{
    public class AdminCommands : Script
    {

        public static readonly List<string> AllAdminCommands = new List<string> { "freeze", "unfreeze", "kick", "invisible", "visible", "tp"};

        public const string COMMAND_FREEZE = "freeze";
        public const string COMMAND_UNFREEZE = "unfreeze";
        public const string COMMAND_KICK= "kick";
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




    }
}
