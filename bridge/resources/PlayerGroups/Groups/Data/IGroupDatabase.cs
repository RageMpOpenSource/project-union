using GTANetworkAPI;
using PlayerGroups.Groups.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerGroups
{
    public interface IGroupDatabase
    {
        PlayerGroupData[] GetGroups();
        string[] GetGroupCommands();
        void AddPlayerToGroup(Client client, int groupId);
        void AddPlayerFromGroup(Client client, int groupId);
        void AddCommandToGroup(string command, int groupId);
        void RemoveCommandFromGroup(string command, int groupId);
    }
}
