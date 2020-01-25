using GTANetworkAPI;
using MySql.Data.MySqlClient;
using ProjectUnion.Utilities;
using System;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Data
{
    public class PlayerDatabase : Script
    {

        public PlayerDatabase()
        {
            CreateTable();
        }

        private async void CreateTable()
        {
            var queryString = "";

            queryString += @"CREATE TABLE characters (
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                name VARCHAR(255) NOT NULL DEFAULT 'John Doe',
                                position_x INT(11) NULL DEFAULT NULL,
                                position_y INT(11) NULL DEFAULT NULL,
                                position_z INT(11) NULL DEFAULT NULL,
                                owner_id int(6) UNSIGNED NOT NULL,
                                cash int(6) UNSIGNED NOT NULL,
                                FOREIGN KEY (owner_id) REFERENCES Users(id)
                             );";

            queryString += @"CREATE TABLE users (
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                name VARCHAR(30) NOT NULL,
                                player_identifier TEXT(255) NOT NULL,
                             );";



            using (var sqlCommand = new MySqlCommand(queryString, ProjectUnion.Database.MySQL.connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    Main.Logger.Log("Player Tables created");
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }
        }


    }
}
