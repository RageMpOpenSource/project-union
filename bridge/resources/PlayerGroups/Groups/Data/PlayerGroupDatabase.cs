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
                                commands LONGTEXT NOT NULL DEFAULT '',
                                group_rank INT(2) UNSIGNED NOT NULL DEFAULT 1
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

        public static async Task<bool> DoesGroupHaveCommand(MySqlConnection connection, string groupName, string command)
        {
            var groupData = await GetGroupData(connection, groupName);
            var query = $@"SELECT `id`, `commands` FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupData.Id} AND `commands` LIKE '%{command}%'";

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
                if (await DoesGroupHaveCommand(connection, group.Name, command)) return true;
            }

            return false;
        }

        public static async Task<long> CreateGroup(MySqlConnection connection, string groupName, System.Drawing.Color groupColor, uint rank = 1, string[] commands = null)
        {
            var groupData = await GetGroupData(connection, groupName);
            if (groupData != null) return groupData.Id;

            if (commands == null) commands = new string[0];

            var query = $@"INSERT INTO `{PLAYER_GROUPS_TABLE}` (name, color, commands, group_rank) VALUES ('{groupName}', {groupColor.ToArgb()}, '{ string.Join(", ", commands)}', {rank})";

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


        public static async Task<bool> RemoveAllPlayersFromGroup(MySqlConnection connection, string groupName)
        {
            var groupData = await GetGroupData(connection, groupName);
            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `group_id` = {groupData.Id}";



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

            var query = $@"SELECT * FROM `{PLAYER_GROUPS_TABLE}` WHERE name = '{groupName}'";

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



        public static async Task<PlayerGroupData> GetPlayerHighestRankingGroup(MySqlConnection connection, uint playerId)
        {

            var query = $@"SELECT B.* FROM {PLAYER_GROUP_ASSIGNMENT_TABLE} AS A INNER JOIN {PLAYER_GROUPS_TABLE} AS B WHERE A.player_id = {playerId} AND B.id = A.group_id";

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
                                    Color = System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString())),
                                    Commands = reader[3].ToString().Trim().Split(","),
                                    Rank = (uint)reader[4]
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

        public static async Task<bool> AddPlayerToGroup(MySqlConnection connection, uint playerId, string groupName)
        {
            if (await IsPlayerInGroup(connection, playerId, groupName)) return false;

            var groupData = await GetGroupData(connection, groupName);



            var query = $@"INSERT INTO `{PLAYER_GROUP_ASSIGNMENT_TABLE}` (group_id, player_id) VALUES ({groupData.Id}, {playerId});";

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


        public static async Task<bool> RemovePlayerFromGroup(MySqlConnection connection, uint playerId, string groupName)
        {
            if (await IsPlayerInGroup(connection, playerId, groupName) == false) return true;
            var groupData = await GetGroupData(connection, groupName);

            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `player_id` = {playerId} AND `group_id` = {groupData.Id};";

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
            var query = $@"SELECT A.* FROM `{PLAYER_GROUPS_TABLE}` as A  INNER JOIN `{PLAYER_GROUP_ASSIGNMENT_TABLE}` as B ON B.player_id = {playerId} AND A.id = B.group_id";

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

        public static async Task<long> DeleteGroup(MySqlConnection connection, string groupName)
        {
            var groupData = await GetGroupData(connection, groupName);
            if (groupData == null) return -1;

            var query = $@"DELETE FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupData.Id}";

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

        public static async Task<bool> AddCommandToGroup(MySqlConnection connection, string groupName, string command)
        {
            var groupData = await GetGroupData(connection, groupName);
            if (Array.IndexOf(groupData.Commands, command) > -1) return true;
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = CONCAT(commands, ', {command}')  WHERE `id` = {groupData.Id} ";

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

        public static async Task<bool> AddCommandsToGroup(MySqlConnection connection, string groupName, string[] commands)
        {
            foreach (var command in commands)
            {
                await AddCommandToGroup(connection, groupName, command);
            }

            return true;
        }


        public static async Task<bool> RemoveCommandsFromGroup(MySqlConnection connection, string groupName, string[] commands)
        {
            foreach (var command in commands)
            {
                await RemoveCommandFromGroup(connection, groupName, command);
            }

            return true;
        }



        public static async Task<bool> RemoveCommandFromGroup(MySqlConnection connection, string groupName, string command)
        {
            var groupData = await GetGroupData(connection, groupName);
            if (Array.IndexOf(groupData.Commands, command) == -1) return false;
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = REPLACE (commands, ', {command}' , '') WHERE `id` = {groupData.Id} ";

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
    }
}
