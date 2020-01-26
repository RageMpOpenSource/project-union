using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Commands
{
    public class GroupCommands : Script
    {
        public static readonly List<string> AllGroupCommands = new List<string> { GET_GROUPS, CREATE_GROUP_COMMAND, DELETE_GROUP_COMMAND, ADD_COMMAND_TO_GROUP_COMMAND, REVOKE_COMMAND_FROM_GROUP, ADD_PLAYER_TO_GROUP_COMMAND, REMOVE_PLAYER_FROM_GROUP_COMMAND };

        public const string CREATE_GROUP_COMMAND = "groupcreate";
        public const string DELETE_GROUP_COMMAND = "groupdelete";
        public const string ADD_COMMAND_TO_GROUP_COMMAND = "groupaddcommand";
        public const string REVOKE_COMMAND_FROM_GROUP = "grouprevokecommand";

        public const string ADD_PLAYER_TO_GROUP_COMMAND = "groupaddplayer";
        public const string REMOVE_PLAYER_FROM_GROUP_COMMAND = "groupremoveplayer";

        public const string GET_GROUPS = "groups";

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


        [Command(GET_GROUPS)]
        public async void CMD_GetGroups(Client client)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, GET_GROUPS) == false) return;

            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_ID);

            var groups = await PlayerGroups.PlayerGroupDatabase.GetPlayerGroups(Database.MySQL.connection, playerData.Id);

            NAPI.Chat.SendChatMessageToPlayer(client, "Groups:");
            string output = "";
            for (int i = 0; i < groups.Length; i++)
            {
                PlayerGroups.Groups.Data.PlayerGroupData groupData = (PlayerGroups.Groups.Data.PlayerGroupData)groups[i];
                output += groupData.Name;
                if (i < groups.Length - 1)
                {
                    output += ", ";
                }
            }

            if (groups.Length == 0)
            {
                output = "You are not part of any groups.";
            }

            NAPI.Chat.SendChatMessageToPlayer(client, output);
        }

        [Command(ADD_PLAYER_TO_GROUP_COMMAND)]
        public async void CMD_AddPlayerToGroup(Client client, string groupName, string playerName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, ADD_COMMAND_TO_GROUP_COMMAND) == false) return;
            if (await VerifyGroup(client, groupName, false) == false) return;
          
            var player = NAPI.Player.GetPlayerFromName(playerName);
            PlayerData playerData = player.GetData(PlayerData.PLAYER_DATA_ID);

            await PlayerGroups.PlayerGroupDatabase.AddPlayerToGroup(ProjectUnion.Database.MySQL.connection, playerData.Id, groupName);
            NAPI.Chat.SendChatMessageToPlayer(client, $"{playerName} has been added to {groupName}");

        }

        [Command(REMOVE_PLAYER_FROM_GROUP_COMMAND)]
        public async void CMD_RemovePlayerFromGroup(Client client, string groupName, string playerName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, REMOVE_PLAYER_FROM_GROUP_COMMAND) == false) return;
            if (await VerifyGroup(client, groupName, false) == false) return;
            
            
            var targetPlayer = NAPI.Player.GetPlayerFromName(playerName);
            var clientPlayerData = client.GetData(PlayerData.PLAYER_DATA_ID);
            var targetPlayerData = targetPlayer.GetData(PlayerData.PLAYER_DATA_ID);

            var targetGroupWithHighestRank = await PlayerGroups.PlayerGroupDatabase.GetPlayerHighestRankingGroup(Database.MySQL.connection, targetPlayerData.Id);
            var clientGroupWithHighestRank = await PlayerGroups.PlayerGroupDatabase.GetPlayerHighestRankingGroup(Database.MySQL.connection, clientPlayerData.Id);


            if (targetPlayerData.Id != clientPlayerData.Id)
            {
                if (clientGroupWithHighestRank.Rank == targetGroupWithHighestRank.Rank)
                {
                    NAPI.Chat.SendChatMessageToPlayer(client, "You cannot remove someone who has the same rank as you.");
                    return;
                }
            }
            else
            {
                if (clientGroupWithHighestRank.Name == groupName)
                {
                    NAPI.Chat.SendChatMessageToPlayer(client, "You cannot remove yourself from your highest group. (iz danger).");
                    return;
                }

            }


            if (clientGroupWithHighestRank.Rank < targetGroupWithHighestRank.Rank)
            {
                NAPI.Chat.SendChatMessageToPlayer(client, "You cannot remove someone who is higher ranking than you.");
                NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $"{client.Name} tried to remove you from group {groupName}.");
                return;
            }


            await PlayerGroups.PlayerGroupDatabase.RemovePlayerFromGroup(ProjectUnion.Database.MySQL.connection, targetPlayerData.Id, groupName);
            NAPI.Chat.SendChatMessageToPlayer(client, $"{targetPlayer.Name} has been removed from {groupName}");
            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $"You have been removed from {groupName} by {client.Name}");
        }


        [Command(ADD_COMMAND_TO_GROUP_COMMAND)]
        public async void CMD_GiveGroupCommannd(Client client, string groupName, string command)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, ADD_COMMAND_TO_GROUP_COMMAND) == false) return;
            if (await VerifyGroup(client, groupName, true) == false) return;

            string message = "";
            bool isCommandAdded = await PlayerGroups.PlayerGroupDatabase.AddCommandToGroup(ProjectUnion.Database.MySQL.connection, groupName, command);

            if (isCommandAdded)
            {
                message = $"{command} has been added to {groupName}.";
            }
            else
            {
                message = $"{groupName} already has command {command}.";
            }

            NAPI.Chat.SendChatMessageToPlayer(client, message);
        }

        [Command(REVOKE_COMMAND_FROM_GROUP)]
        public async void CMD_RevokeGroupCommand(Client client, string groupName, string command)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, REVOKE_COMMAND_FROM_GROUP) == false) return;
            if (await VerifyGroup(client, groupName, true) == false) return;

            string message = "";
            bool isCommandRevoked = await PlayerGroups.PlayerGroupDatabase.RemoveCommandFromGroup(ProjectUnion.Database.MySQL.connection, groupName, command);

            if (isCommandRevoked)
            {
                message = $"{command} has been removed from {groupName}.";
            }
            else
            {
                message = $"{groupName} does not have the command {command}.";
            }

            NAPI.Chat.SendChatMessageToPlayer(client, message);
        }



        [Command(CREATE_GROUP_COMMAND)]
        public async void CMD_CreateGroup(Client client, string groupName, uint rank)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, CREATE_GROUP_COMMAND) == false) return;
            if (await VerifyGroup(client, groupName, false) == false) return;

            await PlayerGroups.PlayerGroupDatabase.CreateGroup(ProjectUnion.Database.MySQL.connection, groupName, System.Drawing.Color.White, rank, new string[0]);
            NAPI.Chat.SendChatMessageToPlayer(client, $"Group {groupName} has been created.");
        }



        [Command(DELETE_GROUP_COMMAND)]
        public async void CMD_DeleteGroup(Client client, string groupName)
        {
            if (await CommandUtilities.VerifyCommandAccess(client, DELETE_GROUP_COMMAND) == false) return;
            if (await VerifyGroup(client, groupName, true) == false) return;

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
