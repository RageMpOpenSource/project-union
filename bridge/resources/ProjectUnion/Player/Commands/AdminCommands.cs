using GTANetworkAPI;
using System.Collections.Generic;

namespace ProjectUnion.Player.Commands
{
    public class AdminCommands : Script
    {

        public static readonly List<string> AllAdminCommands = new List<string> { COMMAND_FREEZE, COMMAND_UNFREEZE, COMMAND_KICK, COMMAND_INVISIBLE, COMMAND_VISIBLE, COMMAND_TELEPORT, COMMAND_HELP, COMMAND_SPAWN_VEH };

        public const string COMMAND_FREEZE = "freeze";
        public const string COMMAND_UNFREEZE = "unfreeze";
        public const string COMMAND_KICK = "kick";
        public const string COMMAND_INVISIBLE = "invisible";
        public const string COMMAND_VISIBLE = "visible";
        public const string COMMAND_TELEPORT = "tp";
        public const string COMMAND_SPAWN_VEH = "veh";
        public const string COMMAND_HELP = "adminhelp";


        [Command(COMMAND_FREEZE)]
        public async void CMD_FreezeTarget(Client client, string targetName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, COMMAND_FREEZE) == false) return;

            var target = NAPI.Player.GetPlayerFromName(targetName);
            NAPI.ClientEvent.TriggerClientEvent(target, "FreezePlayer", true, client.Name);
        }

        [Command(COMMAND_UNFREEZE)]
        public async void CMD_UnFreezeTarget(Client client, string targetName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, COMMAND_UNFREEZE) == false) return;
            var target = NAPI.Player.GetPlayerFromName(targetName);
            NAPI.ClientEvent.TriggerClientEvent(target, "FreezePlayer", false, client.Name);
        }


        [Command(COMMAND_TELEPORT)]
        public async void CMD_Teleport(Client client, string playerFromName, string playerToName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, COMMAND_TELEPORT) == false) return;
            var playerFrom = NAPI.Player.GetPlayerFromName(playerFromName);
            var playerTo = NAPI.Player.GetPlayerFromName(playerToName);

            NAPI.Chat.SendChatMessageToPlayer(playerFrom, $"You have been teleported to {playerTo.Name} by {client.Name}.");
            NAPI.Chat.SendChatMessageToPlayer(client, $"You have teleported {playerFrom.Name} to {playerTo.Name}.");

            NAPI.Player.SpawnPlayer(playerFrom, playerTo.Position.Around(2));
        }

        [Command(COMMAND_SPAWN_VEH)]
        public async void CMD_SpawnVeh(Client client, string vehName, int color1 = 112, int color2 = 112)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, COMMAND_SPAWN_VEH) == false) return;
            NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehName), client.Position.Around(3), client.Heading, color1, color2);
        }


        [Command(COMMAND_HELP)]
        public async void CMD_HelpAdminResponse(Client client)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, COMMAND_HELP) == false) return;
            CommandUtilities.WriteHelpResponse(client, "Admin Commands Help:", AllAdminCommands);
        }

    }
}
