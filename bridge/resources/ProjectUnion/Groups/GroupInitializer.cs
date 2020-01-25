using GTANetworkAPI;
using ProjectUnion.Config;

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
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_ADMIN, System.Drawing.Color.LightYellow);
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_LEAD_ADMIN, System.Drawing.Color.DarkRed);
            PlayerGroups.PlayerGroupDatabase.CreateGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_OWNER, System.Drawing.Color.DarkRed);

            var adminCommands = AdminCommands.AllAdminCommands;

            var leadAdminCommands = AdminCommands.AllAdminCommands;
            leadAdminCommands.AddRange(GroupCommands.AllGroupCommands);

            var ownerCommands = leadAdminCommands;

            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_ADMIN, adminCommands.ToArray());
            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_LEAD_ADMIN, leadAdminCommands.ToArray());
            PlayerGroups.PlayerGroupDatabase.AddCommandsToGroup(Database.MySQL.connection, GroupConfig.GROUP_NAME_OWNER, ownerCommands.ToArray());
        }
    }
}
