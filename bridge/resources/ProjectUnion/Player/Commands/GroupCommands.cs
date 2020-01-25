using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Commands
{
    public class GroupCommands : Script
    {
        public static readonly List<string> AllGroupCommands = new List<string> { CREATE_GROUP_COMMAND, DELETE_GROUP_COMMAND, ADD_COMMAND_TO_GROUP_COMMAND, REVOKE_COMMAND_TO_GROUP_COMMAND };

        public const string CREATE_GROUP_COMMAND = "groupcreate";
        public const string DELETE_GROUP_COMMAND = "groupdelete";
        public const string ADD_COMMAND_TO_GROUP_COMMAND = "groupaddcommand";
        public const string REVOKE_COMMAND_TO_GROUP_COMMAND = "grouprevokecommand";

        public const string ADD_PLAYER_TO_GROUP_COMMAND = "groupaddplayer";
        public const string REMOVE_PLAYER_FROM_GROUP_COMMAND = "groupremoveplayer";

        public async Task<bool> VerifyGroup(Client client, string groupName, bool exists)
        {
            if (await PlayerGroups.PlayerGroupDatabase.GetGroupData(Database.MySQL.connection, groupName) == null)
            {
                if (exists)
                {
                    return true;
                }

                NAPI.Chat.SendChatMessageToPlayer(client, $"Group '{groupName}' not found.");
            }
            else
            {
                if (!exists)
                {
                    return true;
                }

                NAPI.Chat.SendChatMessageToPlayer(client, $"Group '{groupName}' already exists.");
            }
            return false;
        }


        [Command(ADD_COMMAND_TO_GROUP_COMMAND)]
        public async void CMD_GiveGroupCommansd(Client client, string groupName, string command)
        {
            if (await VerifyGroup(client, groupName, true) == false) return;
            if (await CommandUtilities.VerifyCommandAccess(client, ADD_COMMAND_TO_GROUP_COMMAND) == false) return;

            await PlayerGroups.PlayerGroupDatabase.AddCommandToGroup(ProjectUnion.Database.MySQL.connection, groupName, command);
            NAPI.Chat.SendChatMessageToPlayer(client, $"{command} has been added to {groupName}");

        }

        [Command(REVOKE_COMMAND_TO_GROUP_COMMAND)]
        public async void CMD_RevokeGroupCommand(Client client, string groupName, string command)
        {
            if (await VerifyGroup(client, groupName, true) == false) return;
            if (await CommandUtilities.VerifyCommandAccess(client, REVOKE_COMMAND_TO_GROUP_COMMAND) == false) return;


            await PlayerGroups.PlayerGroupDatabase.RemoveCommandFromGroup(ProjectUnion.Database.MySQL.connection, groupName, command);
            NAPI.Chat.SendChatMessageToPlayer(client, $"{command} has been removed from {groupName}");
        }



        [Command(CREATE_GROUP_COMMAND)]
        public async void CMD_CreateGroup(Client client, string groupName)
        {
            if (await VerifyGroup(client, groupName, false) == false) return;
            if (await CommandUtilities.VerifyCommandAccess(client, CREATE_GROUP_COMMAND) == false) return;

            await PlayerGroups.PlayerGroupDatabase.CreateGroup(ProjectUnion.Database.MySQL.connection, groupName, System.Drawing.Color.White, new string[0]);
            NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} has been created.");
        }



        [Command(DELETE_GROUP_COMMAND)]
        public async void CMD_DeleteGroup(Client client, string groupName)
        {
            if (await VerifyGroup(client, groupName, true) == false) return;
            if (await CommandUtilities.VerifyCommandAccess(client, DELETE_GROUP_COMMAND) == false) return;

            await PlayerGroups.PlayerGroupDatabase.DeleteGroup(ProjectUnion.Database.MySQL.connection, groupName);
            NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} has been removed.");
        }



        [Command("grouphelp")]
        public void CMD_HelpAdminResponse(Client client)
        {
            CommandUtilities.WriteHelpResponse(client, "Group Commands Help:", AllGroupCommands);
        }

     
    }
}
