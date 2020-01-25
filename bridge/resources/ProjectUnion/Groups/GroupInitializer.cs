using GTANetworkAPI;
using ProjectUnion.Config;
using System.Collections.Generic;

namespace ProjectUnion.Player.Commands
{
    class GroupInitializer : Script
    {

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            CreateGroups();
        }

        private void CreateGroups()
        {
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_PLAYER, System.Drawing.Color.White, 1);
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_ADMIN, System.Drawing.Color.LightYellow, 10);
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_LEAD_ADMIN, System.Drawing.Color.DarkRed, 13);
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_OWNER, System.Drawing.Color.DarkRed, 20);

            var playerCommands = new List<string>() { GroupCommands.GET_GROUPS };

            var adminCommands = playerCommands;
            adminCommands.AddRange(AdminCommands.AllAdminCommands);

            var leadAdminCommands = adminCommands;
            leadAdminCommands.AddRange(GroupCommands.AllGroupCommands);

            var ownerCommands = leadAdminCommands;

            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_PLAYER, playerCommands.ToArray());
            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_ADMIN, adminCommands.ToArray());
            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_LEAD_ADMIN, leadAdminCommands.ToArray());
            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_OWNER, ownerCommands.ToArray());
        }
    }
}
