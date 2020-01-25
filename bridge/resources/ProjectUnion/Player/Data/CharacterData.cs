﻿using System;
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
        public static async Task<CharacterData> GetCharacterData(Client client, int characterId)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            var query = $"SELECT * FROM characters WHERE owner_id={playerData.Id} AND id ={characterId} LIMIT 1";
            using (MySqlCommand mySqlCommand = new MySqlCommand(query, Database.MySQL.connection))
            {
                using (var reader = await mySqlCommand.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        var pos = new Vector3((float)reader[2], (float)reader[3], (float)reader[4]);

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
                    return await GetCharacterData(client, (int)mySqlCommand.LastInsertedId);
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
