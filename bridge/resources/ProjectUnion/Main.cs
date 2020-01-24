using GTANetworkAPI;
using ProjectUnion.Database;
using ProjectUnion.Utilities;
using System;

namespace ProjectUnion
{
    public class Main : Script
    {
        public static Logger Logger = new Logger();
        public static Random Random = new Random();
        public Main()
        {
            MySQL mysql = new MySQL();
            mysql.Connect();

            PlayerGroups.Main.Initialise(MySQL.connection);

            RunTasks();
        }

        private async void RunTasks()
        {
            //await PlayerGroups.PlayerGroupDatabase.CreateGroup(MySQL.connection, "Owner", System.Drawing.Color.Red, new string[] { "sit", "lie", "jump" });
            //await PlayerGroups.PlayerGroupDatabase.CreateGroup(MySQL.connection, "Admin", System.Drawing.Color.Yellow, new string[] { "sit", "lie", "jump" });
        }



    }
}
