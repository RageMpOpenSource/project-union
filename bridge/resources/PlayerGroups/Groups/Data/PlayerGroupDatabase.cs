using GTANetworkAPI;
using MySql.Data.MySqlClient;
using PlayerGroups.Groups.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayerGroups
{
    public class PlayerGroupDatabase
    {
        private const string PLAYER_GROUPS_TABLE = "player_groups";
        private const string PLAYER_GROUP_ASSIGNMENT_TABLE = "player_group_assignment";

        public static async Task<bool> CreateTable(MySqlConnection connection)
        {

            var queryString = $@"CREATE TABLE {PLAYER_GROUPS_TABLE}(
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                name VARCHAR(255) UNIQUE NOT NULL DEFAULT 'Simple Name',
                                color INT(255) NOT NULL DEFAULT 0,
                                commands VARCHAR(255) NOT NULL DEFAULT ''
                             );";


            queryString += $@"CREATE TABLE {PLAYER_GROUP_ASSIGNMENT_TABLE}(
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                group_id INT(6) UNSIGNED NOT NULL,
                                player_id INT(6) UNSIGNED NOT NULL,
                                FOREIGN KEY (group_id) REFERENCES {PLAYER_GROUPS_TABLE}(id),
                                FOREIGN KEY (player_id) REFERENCES Users(id)
                             );";

            using (var sqlCommand = new MySqlCommand(queryString, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        public static async Task<bool> HasCommand(MySqlConnection connection, uint groupId, string command)
        {
            var query = $@"SELECT `id`, `commands` FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupId} AND `commands` LIKE '%{command}%'";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        return reader.HasRows;
                    }
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        public static async Task<bool> DoesPlayerHaveCommand(MySqlConnection connection, uint playerId, string command)
        {
            var playerGroups = await GetPlayerGroups(connection, playerId);
            foreach (var group in playerGroups)
            {
                if (await HasCommand(connection, group.Id, command)) return true;
            }

            return false;
        }

        public static async Task<long> CreateGroup(MySqlConnection connection, string groupName, System.Drawing.Color groupColor, string[] commands)
        {
            var query = $@"INSERT INTO `{PLAYER_GROUPS_TABLE}` (name, color, commands) VALUES ('{groupName}', {groupColor.ToArgb()}, '{ string.Join(", ", commands)}')";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return sqlCommand.LastInsertedId;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return -1;
        }

        public static async Task<bool> RemoveAllPlayersFromGroup(MySqlConnection connection, int groupId)
        {
            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `group_id` = {groupId}";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }


        public static async Task<bool> RemovePlayerFromAllGroups(MySqlConnection connection, int playerId)
        {
            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `player_id` = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        public static async Task<PlayerGroupData> GetGroupData(MySqlConnection connection, string groupName)
        {

            var query = $@"SELECT * FROM `{PLAYER_GROUPS_TABLE}` WHERE name = `{groupName}`";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {

                        try
                        {

                            while (await reader.ReadAsync())
                            {
                                var group = new PlayerGroupData()
                                {
                                    Id = (uint)reader[0],
                                    Name = reader[1].ToString(),
                                    Commands = reader[3].ToString().Trim().Split(","),
                                    Color = System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString()))
                                };
                                return group;
                            }

                        }
                        catch (Exception e)
                        {
                            NAPI.Util.ConsoleOutput(e.ToString());
                        }


                    }
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return null;
        }

        public static async Task<bool> IsPlayerInGroup(MySqlConnection connection, uint playerId, string groupName)
        {
            var query = $@"SELECT * FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` AS A INNER JOIN `{PLAYER_GROUPS_TABLE}` AS B WHERE B.id = A.group_id AND B.name = '{groupName}' AND A.player_id = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        return reader.HasRows;
                    }
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }


        public static async Task<bool> IsPlayerInGroup(MySqlConnection connection, uint playerId, uint groupId)
        {
            var query = $@"SELECT * FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `group_id` = {groupId} AND `player_id` = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        return reader.HasRows;
                    }
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        public static async Task<bool> AddPlayerToGroup(MySqlConnection connection, uint playerId, uint groupId)
        {
            if (await IsPlayerInGroup(connection, playerId, groupId)) return false;

            var query = $@"INSERT INTO `{PLAYER_GROUP_ASSIGNMENT_TABLE}` (group_id, player_id) VALUES ({groupId}, {playerId});";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }


        public static async Task<bool> RemovePlayerFromGroup(MySqlConnection connection, uint playerId, uint groupId)
        {
            if (await IsPlayerInGroup(connection, playerId, groupId) == false) return true;

            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `player_id` = {playerId} AND `group_id` = {groupId};";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }



        public static async Task<PlayerGroupData[]> GetPlayerGroups(MySqlConnection connection, uint playerId)
        {
            var query = $@"SELECT A.* FROM `{PLAYER_GROUPS_TABLE}` as A  INNER JOIN `{PLAYER_GROUP_ASSIGNMENT_TABLE}` as B ON B.player_id = 0 AND A.id = B.group_id";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        var groups = new List<PlayerGroupData>();

                        try
                        {

                            while (await reader.ReadAsync())
                            {
                                var group = new PlayerGroupData()
                                {
                                    Id = (uint)reader[0],
                                    Name = reader[1].ToString(),
                                    Commands = reader[3].ToString().Trim().Split(","),
                                    Color = System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString()))
                                };

                                groups.Add(group);
                            }

                            return groups.ToArray();
                        }
                        catch (Exception e)
                        {
                            NAPI.Util.ConsoleOutput(e.ToString());
                        }


                    }
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return null;
        }

        public static async Task<long> RemoveGroup(MySqlConnection connection, uint groupId)
        {
            var query = $@"DELETE FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupId}";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return sqlCommand.LastInsertedId;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return -1;
        }

        public static async Task<bool> AddCommandToGroup(MySqlConnection connection, uint groupId, string command)
        {
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = CONCAT(commands, ', {command}')  WHERE `id` = {groupId} ";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        public static async Task<bool> RemoveCommandFromGroup(MySqlConnection connection, uint groupId, string command)
        {
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = REPLACE (commands, ', {command}' , '') WHERE `id` = {groupId} ";

            using (var sqlCommand = new MySqlCommand(query, connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }

            return false;
        }

        //PlayerGroupData[] GetGroups()
        //{
        //    throw new NotImplementedException();
        //}

        //string[] GetGroupCommands()
        //{
        //    throw new NotImplementedException();
        //}

        //void AddPlayerToGroup(Client client, int groupId)
        //{
        //    throw new NotImplementedException();
        //}

        //void AddPlayerFromGroup(Client client, int groupId)
        //{
        //    throw new NotImplementedException();
        //}

        //void AddCommandToGroup(string command, int groupId)
        //{
        //    throw new NotImplementedException();
        //}

        //void RemoveCommandFromGroup(string command, int groupId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
