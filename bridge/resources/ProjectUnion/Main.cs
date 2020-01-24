using GTANetworkAPI;
using ProjectUnionFreeroam.Database;
using ProjectUnionFreeroam.Utilities;
using System;

namespace ProjectUnionFreeroam
{
    public class Main : Script
    {
        public static Logger Logger = new Logger();
        public static Random Random = new Random();
        public Main()
        {
            MySQL mysql = new MySQL();
            mysql.Connect();
        }



    }
}
