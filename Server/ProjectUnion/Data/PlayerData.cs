using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Data
{
    public class PlayerTempData
    {
        public const string PLAYER_TEMP_DATA_KEY = "PLAYER_TEMP_DATA_KEY";
        public uint LoginIndex { get; set; }

        public uint? GamemodeId { get; set; }
    }

    public class PlayerData
    {
        public const string PLAYER_DATA_KEY = "PLAYER_DATA_KEY";
        public uint Id { get; set; }
        public string PlayerHash { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public static class PlayerDatabase
    {
        public static async void InitializeTable()
        {
            string query = $@"CREATE TABLE IF NOT EXISTS `Users` (
                            `id` int UNSIGNED AUTO_INCREMENT PRIMARY KEY NOT NULL,
                            `player_hash` varchar(255) NULL,
                            `last_login` varchar(255) NULL
                            );";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();

                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }
        }

        public static async Task<PlayerData> CreatePlayer(PlayerData playerData)
        {
            string query = $@"INSERT INTO Users(`player_hash`, `last_login`) VALUES ('{playerData.PlayerHash}', '{playerData.LastLogin.ToString()}');";


            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    playerData.Id = (uint)command.LastInsertedId;
                    return playerData;

                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return null;
        }


        public static async void SavePlayer(PlayerData playerData)
        {
            string query = $@"UPDATE Users SET 
                                `player_hash` = '{playerData.PlayerHash}',
                                `last_login` = '{playerData.LastLogin.ToString()}'  WHERE id = {playerData.Id}";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();

                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }
        }

        public static async Task<PlayerData> GetPlayerData(uint playerId)
        {

            string query = $"SELECT * FROM `users` WHERE `id` = {playerId}";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            PlayerData playerData = new PlayerData()
                            {
                                Id = (uint)reader[0],
                                PlayerHash = reader[1].ToString(),
                                LastLogin = Convert.ToDateTime(reader[2].ToString())
                            };
                            return playerData;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return null;
        }

        public static async Task<PlayerData> GetPlayerData(string address)
        {

            string query = $"SELECT * FROM `users` WHERE `player_hash` = '{address}'";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            PlayerData playerData = new PlayerData()
                            {
                                Id = (uint)reader[0],
                                PlayerHash = reader[1].ToString(),
                                LastLogin = Convert.ToDateTime(reader[2].ToString())
                            };
                            return playerData;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return null;
        }
    }
}
