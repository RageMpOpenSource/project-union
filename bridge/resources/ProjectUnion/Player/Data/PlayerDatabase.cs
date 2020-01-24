using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

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
            var queryString = @"CREATE TABLE users (
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                name VARCHAR(30) NOT NULL,
                                player_hash TEXT(255) NOT NULL,
                                char_id int(8)
                             );";

            queryString += @"CREATE TABLE characters (
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                owner_id int(6) UNSIGNED NOT NULL,
                                FOREIGN KEY (owner_id) REFERENCES Users(id)
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
