using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectUnion.Data
{

    public class GroupData
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public string[] Commands { get; set; }
        public uint Rank { get; set; }


    }


    public class GroupDatabase
    {

        private static Color ToColor(System.Drawing.Color c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        private const string PLAYER_GROUPS_TABLE = "player_groups";
        private const string PLAYER_GROUP_ASSIGNMENT_TABLE = "player_group_assignment";

        public static async void InitializeTable()
        {

            var queryString = $@"CREATE TABLE IF NOT EXISTS {PLAYER_GROUPS_TABLE}(
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                name VARCHAR(255) UNIQUE NOT NULL DEFAULT 'Simple Name',
                                color INT(255) NOT NULL DEFAULT 0,
                                commands LONGTEXT NOT NULL DEFAULT '',
                                group_rank INT UNSIGNED NOT NULL DEFAULT 1
                             );";


            queryString += $@"CREATE TABLE IF NOT EXISTS {PLAYER_GROUP_ASSIGNMENT_TABLE}(
                                id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                                group_id INT(6) UNSIGNED NOT NULL,
                                player_id INT(6) UNSIGNED NOT NULL,
                                FOREIGN KEY (group_id) REFERENCES {PLAYER_GROUPS_TABLE}(id),
                                FOREIGN KEY (player_id) REFERENCES Users(id)
                             );";

            using (var sqlCommand = new MySqlCommand(queryString, Main.Connection))
            {
                try
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.Message);
                }
            }
        }

        public static async void InitializeGroups()
        {
            await CreateGroup(Config.GROUP_NAME_ADMIN, System.Drawing.Color.LightGoldenrodYellow, 5);
            await CreateGroup(Config.GROUP_NAME_LEAD_ADMIN, System.Drawing.Color.DarkRed, 10);
            await CreateGroup(Config.GROUP_NAME_OWNER, System.Drawing.Color.IndianRed, 30);
        }

        public static async Task<bool> DoesGroupHaveCommand(string groupName, string command)
        {
            var groupData = await GetGroupData(groupName);
            var query = $@"SELECT `id`, `commands` FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupData.Id} AND `commands` LIKE '%{command}%'";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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

        public static async Task<bool> DoesPlayerHaveCommand(uint playerId, string command)
        {
            var playerGroups = await GetPlayerGroups(playerId);
            foreach (var group in playerGroups)
            {
                if (await DoesGroupHaveCommand(group.Name, command)) return true;
            }

            return false;
        }

        public static async Task<long> CreateGroup(string groupName, System.Drawing.Color groupColor, uint rank = 1, string[] commands = null)
        {
            var groupData = await GetGroupData(groupName);
            if (groupData != null) return groupData.Id;

            if (commands == null) commands = new string[0];

            var query = $@"INSERT INTO `{PLAYER_GROUPS_TABLE}` (name, color, commands, group_rank) VALUES ('{groupName}', {groupColor.ToArgb()}, '{ string.Join(", ", commands)}', {rank})";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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


        public static async Task<bool> RemoveAllPlayersFromGroup(string groupName)
        {
            var groupData = await GetGroupData(groupName);
            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `group_id` = {groupData.Id}";



            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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


        public static async Task<bool> RemovePlayerFromAllGroups(int playerId)
        {
            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `player_id` = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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

        public static async Task<GroupData> GetGroupData(string groupName)
        {

            var query = $@"SELECT * FROM `{PLAYER_GROUPS_TABLE}` WHERE name = '{groupName}'";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {

                        try
                        {

                            while (await reader.ReadAsync())
                            {
                                var group = new GroupData()
                                {
                                    Id = (uint)reader[0],
                                    Name = reader[1].ToString(),
                                    Commands = reader[3].ToString().Trim().Split(","),
                                    Color = ToColor(System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString())))
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



        public static async Task<GroupData> GetPlayerHighestRankingGroup(uint playerId)
        {

            var query = $@"SELECT B.* FROM {PLAYER_GROUP_ASSIGNMENT_TABLE} AS A INNER JOIN {PLAYER_GROUPS_TABLE} AS B WHERE A.player_id = {playerId} AND B.id = A.group_id";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {

                        try
                        {

                            while (await reader.ReadAsync())
                            {
                                var group = new GroupData()
                                {
                                    Id = (uint)reader[0],
                                    Name = reader[1].ToString(),
                                    Color = ToColor(System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString()))),
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


        public static async Task<bool> IsPlayerInGroup(uint playerId, string groupName)
        {
            var query = $@"SELECT * FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` AS A INNER JOIN `{PLAYER_GROUPS_TABLE}` AS B WHERE B.id = A.group_id AND B.name = '{groupName}' AND A.player_id = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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


        public static async Task<bool> IsPlayerInGroup(uint playerId, uint groupId)
        {
            var query = $@"SELECT * FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `group_id` = {groupId} AND `player_id` = {playerId}";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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

        public static async Task<bool> AddPlayerToGroup(uint playerId, string groupName)
        {
            if (await IsPlayerInGroup(playerId, groupName)) return false;
            if (await PlayerDatabase.GetPlayerData(playerId) == null) return false;

            var groupData = await GetGroupData(groupName);

            var query = $@"INSERT INTO `{PLAYER_GROUP_ASSIGNMENT_TABLE}` (group_id, player_id) VALUES ({groupData.Id}, {playerId});";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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


        public static async Task<bool> RemovePlayerFromGroup(uint playerId, string groupName)
        {
            if (await IsPlayerInGroup(playerId, groupName) == false) return true;
            var groupData = await GetGroupData(groupName);

            var query = $@"DELETE FROM `{PLAYER_GROUP_ASSIGNMENT_TABLE}` WHERE `player_id` = {playerId} AND `group_id` = {groupData.Id};";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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



        public static async Task<GroupData[]> GetPlayerGroups(uint playerId)
        {
            var query = $@"SELECT A.* FROM `{PLAYER_GROUPS_TABLE}` as A  INNER JOIN `{PLAYER_GROUP_ASSIGNMENT_TABLE}` as B ON B.player_id = {playerId} AND A.id = B.group_id";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        var groups = new List<GroupData>();

                        try
                        {

                            while (await reader.ReadAsync())
                            {
                                var group = new GroupData()
                                {
                                    Id = (uint)reader[0],
                                    Name = reader[1].ToString(),
                                    Commands = reader[3].ToString().Trim().Split(","),
                                    Color = ToColor(System.Drawing.Color.FromArgb(int.Parse(reader[2].ToString())))
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

        public static async Task<long> DeleteGroup(string groupName)
        {
            var groupData = await GetGroupData(groupName);
            if (groupData == null) return -1;

            var query = $@"DELETE FROM `{PLAYER_GROUPS_TABLE}` WHERE `id` = {groupData.Id}";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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

        public static async Task<bool> AddCommandToGroup(string groupName, string command)
        {
            var groupData = await GetGroupData(groupName);
            if (Array.IndexOf(groupData.Commands, command) > -1) return true;
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = CONCAT(commands, ', {command}')  WHERE `id` = {groupData.Id} ";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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

        public static async Task<bool> AddCommandsToGroup(string groupName, string[] commands)
        {
            foreach (var command in commands)
            {
                await AddCommandToGroup(groupName, command);
            }

            return true;
        }


        public static async Task<bool> RemoveCommandsFromGroup(string groupName, string[] commands)
        {
            foreach (var command in commands)
            {
                await RemoveCommandFromGroup(groupName, command);
            }

            return true;
        }



        public static async Task<bool> RemoveCommandFromGroup(string groupName, string command)
        {
            var groupData = await GetGroupData(groupName);
            if (Array.IndexOf(groupData.Commands, command) == -1) return false;
            var query = $@"UPDATE `{PLAYER_GROUPS_TABLE}` SET `commands` = REPLACE (commands, ', {command}' , '') WHERE `id` = {groupData.Id} ";

            using (var sqlCommand = new MySqlCommand(query, Main.Connection))
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
