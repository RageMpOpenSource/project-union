using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace ProjectUnion.Player.Data
{

    /// <summary>
    /// This class contains the Data for the characters a master account can hold.
    /// Every player can have x number of characters in 1 master account.
    /// </summary>

    public class CharacterData
    {
        public const string CHARACTER_DATA_ID = "CHARACTER_DATA_ID";

        /// <summary>The SQL ID for a character is stored in this attribute.</summary>
        public uint Id { get; set; }
        /// <summary>The Master account ID which is linked to the current character is stored in this attribute.</summary>
        public uint PlayerId { get; set; }
        public string Name { get; set; }
        /// <summary>Private attribute for spawning a character.</summary>
        private Vector3 _spawnPosition;


        /// <summary>
        /// This holds the position of the character's spawn location.
        /// <returns>The getter gets the <see cref="Vector3"/> last updated spawn poosition.</returns>
        /// <param>The setter sets the <see cref="Vector3"/> position of a player and updates the coloumn for the specific player.</param>
        /// </summary>
        public Vector3 SpawnPosition
        {
            get
            {
                return this._spawnPosition;
            }
            set
            {
                this._spawnPosition = value;
            }
        }


        /// <summary>Contains the cash the character of the player has.</summary>
        private long _cash;
        /// <summary>
        /// Contains the cash of the chatacter of the player.
        /// <param>Sets the cash of the player. And updates the DB data.</param>
        /// <returns>Gets the latest updated cash of the character.</returns>
        /// </summary>
        public long Cash
        {
            get
            {
                return this._cash;
            }
            set
            {
                // TO-DO: make a function for updating character data and update the cash by invoking the function //
                // And to update a scaleform as well //
                this._cash = value;
            }
        }



        #region Database 

        public static async Task<CharacterData[]> GetAllCharacters(uint playerId)
        {
            var query = $"SELECT * FROM characters WHERE owner_id={playerId} ";
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                using (var reader = await mySqlCommand.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        List<CharacterData> characters = new List<CharacterData>();
                        while (reader.Read())
                        {
                            Vector3 pos = null;
                            if (string.IsNullOrEmpty(reader[2].ToString()) == false)
                            {
                                pos = new Vector3(float.Parse(reader[2].ToString()), float.Parse(reader[3].ToString()), float.Parse(reader[4].ToString()));
                            }

                            var charData = new CharacterData()
                            {
                                Id = (uint)reader[0],
                                Name = (string)reader[1],
                                SpawnPosition = pos,
                                PlayerId = (uint)reader[5],
                                Cash = (uint)reader[6],

                            };

                            characters.Add(charData);
                        }
                        return characters.ToArray();
                    }
                }
            }

            return null;
        }


        public static async void Save(CharacterData data)
        {
            var query = $"UPDATE characters SET ";

            query += $"name='{data.Name}'";

            if (data.SpawnPosition != null)
            {
                query += ",";
                query += $@"position_x='{data.SpawnPosition.X.ToString()}', 
                            position_y='{data.SpawnPosition.Y.ToString()}',
                            position_z='{data.SpawnPosition.Z.ToString()}'
                            ";
            }

            query += $" WHERE id = {data.Id}";

            NAPI.Util.ConsoleOutput(query);
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                try
                {
                    await mySqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.ToString());
                }

            }
        }



        public static async Task<CharacterData> GetCharacterData(Client client, uint characterId)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            var query = $"SELECT * FROM characters WHERE owner_id={playerData.Id} AND id ={characterId} LIMIT 1";
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                using (var reader = await mySqlCommand.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        Vector3 pos = null;
                        if (string.IsNullOrEmpty(reader[2].ToString()) == false)
                        {
                            pos = new Vector3(float.Parse(reader[2].ToString()), float.Parse(reader[3].ToString()), float.Parse(reader[4].ToString()));
                        }


                        return new CharacterData()
                        {
                            Id = (uint)reader[0],
                            Name = (string)reader[1],
                            SpawnPosition = pos,
                            PlayerId = (uint)reader[5],
                            Cash = (uint)reader[6],

                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public async static Task<CharacterData> CreateCharacterData(Client client)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            var query = $"INSERT INTO characters (owner_id) VALUES ({playerData.Id});";

            NAPI.Util.ConsoleOutput("CREATE CHARACTER");
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                try
                {
                    await mySqlCommand.ExecuteNonQueryAsync();
                    return await GetCharacterData(client, (uint)mySqlCommand.LastInsertedId);
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
