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
        }


    }
}
