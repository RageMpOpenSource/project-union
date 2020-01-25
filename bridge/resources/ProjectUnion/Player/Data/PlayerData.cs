using GTANetworkAPI;
using MySql.Data.MySqlClient;
using ProjectUnion.Utilities;
using System;
using System.Threading.Tasks;

namespace ProjectUnion.Player.Data
{
    public class PlayerData
    {
        public const string PLAYER_DATA = "PLAYER_DATA";
        public uint Id { get; set; }

        //TODO: Hook up Character Id with Database [Get only one]
        public uint CurrentCharacterId { get; set; }


        #region Database Handling
        public static async Task<PlayerData> GetPlayerData(Client client)
        {
            var address = NAPI.Player.GetPlayerAddress(client);
            var query = $"SELECT * FROM users WHERE player_identifier='{address}' LIMIT 1";
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                using (var reader = await mySqlCommand.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        return new PlayerData()
                        {
                            Id = (uint)reader[0]
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public async static Task<PlayerData> CreatePlayerData(Client client)
        {
            var address = NAPI.Player.GetPlayerAddress(client);
            var query = $"INSERT INTO users (name, player_identifier) VALUES ('{client.Name}', '{address}');";

            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                try
                {
                    await mySqlCommand.ExecuteNonQueryAsync();

                    //Create Character Data
                    var character = CharacterData.CreateCharacterData(client);

                    return await GetPlayerData(client);
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.ToString());
                }
            }

            return null;
        }

        #endregion
    }
}
