using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace PlayerGroups
{
    public class Main : Script
    {

        public static async void Initialise(MySqlConnection connection)
        {
            await PlayerGroupDatabase.CreateTable(connection);
        }


    }
}
