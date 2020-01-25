using GTANetworkAPI;

namespace ProjectUnion.Player
{
    public class Commands : Script
    {
        //TODO: Centralize no access response


        [Command("givegroupcommand")]
        public async void CMD_GiveGroupCommansd(Client client, string groupName, string cmdName)
        {
            if (await PlayerGroups.PlayerGroupDatabase.GetGroupData(Database.MySQL.connection, groupName) == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} not found.");
                return;
            }

            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(ProjectUnion.Database.MySQL.connection, 0, "giveGroupCommand"))
            {
                await PlayerGroups.PlayerGroupDatabase.AddCommandToGroup(ProjectUnion.Database.MySQL.connection, groupName, cmdName);
                NAPI.Chat.SendChatMessageToPlayer(client, $"{cmdName} has been added to {groupName}");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"You don't have access to this command.");
            }

        }

        [Command("revokegroupcommand")]
        public async void CMD_RevokeGroupCommand(Client client, string groupName, string cmdName)
        {
            if (await PlayerGroups.PlayerGroupDatabase.GetGroupData(Database.MySQL.connection, groupName) == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} not found.");
                return;
            }

            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(ProjectUnion.Database.MySQL.connection, 0, "revokeGroupCommand"))
            {
                await PlayerGroups.PlayerGroupDatabase.RemoveCommandFromGroup(ProjectUnion.Database.MySQL.connection, groupName, cmdName);
                NAPI.Chat.SendChatMessageToPlayer(client, $"{cmdName} has been removed from {groupName}");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"You don't have access to this command.");
            }
        }



        [Command("creategroup")]
        public async void CMD_CreateGroup(Client client, string groupName)
        {
            if (await PlayerGroups.PlayerGroupDatabase.GetGroupData(Database.MySQL.connection, groupName) != null)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"Group with name {groupName} already exists.");
                return;
            }

            if (await PlayerGroups.PlayerGroupDatabase.DoesPlayerHaveCommand(ProjectUnion.Database.MySQL.connection, 0, "creategroup"))
            {
                await PlayerGroups.PlayerGroupDatabase.CreateGroup(ProjectUnion.Database.MySQL.connection, groupName, System.Drawing.Color.White, new string[] { "test", "test2" });
                NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} has been created.");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(client, $"You don't have access to this command.");
            }
        }





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
