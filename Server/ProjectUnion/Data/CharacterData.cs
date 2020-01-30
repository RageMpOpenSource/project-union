using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace ProjectUnion.Data
{
    public class CharacterData
    {
        public const string CHARACTER_DATA_KEY = "CHARACTER_DATA_KEY";
        public uint Id { get; set; }
        public string Name { get; set; } = "John Doe";
        public uint OwnerId { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public float? PositionZ { get; set; }
        public float? Heading { get; set; }

   
        public Vector3 GetPosition()
        {
            if (PositionX.HasValue && PositionY.HasValue && PositionZ.HasValue)
            {
                return new Vector3(PositionX.Value, PositionY.Value, PositionZ.Value);
            }

            return null;
        }
    }

    public static class CharacterDatabase
    {
        public static async void InitializeTable()
        {
            string query = $@"CREATE TABLE IF NOT EXISTS `Characters` (
                            `id` int UNSIGNED AUTO_INCREMENT PRIMARY KEY NOT NULL,
                            `owner_id` int UNSIGNED NOT NULL,
                            `name` varchar(255) NOT NULL DEFAULT 'John Doe',
                            `position_x` LONGTEXT NULL,
                            `position_y` LONGTEXT NULL,
                            `position_z` LONGTEXT NULL,
                            `heading` LONGTEXT NULL,
                            FOREIGN KEY(owner_id) REFERENCES Users(id)
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

        public static async Task<CharacterData> CreateCharacter(CharacterData characterData)
        {
            string query = $@"INSERT INTO Characters (`name`, `owner_id`) VALUES ('{characterData.Name}', {characterData.OwnerId});";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    characterData.Id = (uint)command.LastInsertedId;
                    return characterData;

                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return null;
        }


        public static async void SaveCharacter(CharacterData characterData)
        {
            string query = $@"UPDATE Characters SET 
                                `name` = '{characterData.Name}',
                                `position_x` = '{characterData.PositionX}', 
                                `position_y` = '{characterData.PositionY}', 
                                `position_z` = '{characterData.PositionZ}', 
                                `heading` = '{characterData.Heading}' 
                                WHERE id = {characterData.Id}";

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


        public static async Task<uint[]> GetCharacters(uint ownerId)
        {
            string query = $"SELECT * FROM `characters` WHERE `owner_id` = {ownerId}";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            List<uint> characterIds = new List<uint>();
                            while (reader.Read())
                            {
                                characterIds.Add((uint)reader[0]);
                            }
                            return characterIds.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return new uint[0];
        }

        public static async Task<CharacterData> GetCharacterData(uint id)
        {

            string query = $"SELECT * FROM `characters` WHERE `id` = {id}";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            CharacterData characterData = new CharacterData()
                            {
                                Id = (uint)reader[0],
                                OwnerId = (uint)reader[1],
                                Name = reader[2].ToString(),
                                PositionX = reader.FloatOrNull(3),
                                PositionY = reader.FloatOrNull(4),
                                PositionZ = reader.FloatOrNull(5),
                                Heading = reader.FloatOrNull(6),
                            };

                            return characterData;
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
