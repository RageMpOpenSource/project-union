using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace ProjectUnion
{
    public class Database
    {
        public MySqlConnection Connection;


        public Database()
        {
            string connectionString = "Server=localhost;Uid=root;password=;Database=projectunion;";
            Connection = new MySqlConnection(connectionString);

            try
            {
                Connection.Open();
            }
            catch (Exception e)
            {
                Main.Logger.LogError(e.Message);
            }
        }
    }
}
