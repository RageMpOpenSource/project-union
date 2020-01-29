using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Data
{
    public class VehicleData
    {
        public const string VEHICLE_DATA_KEY = "VEHICLE_DATA_KEY";
        public uint Id { get; set; }
        public string VehicleName { get; set; }
        public int Color1 { get; set; }
        public int Color2 { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public float? PositionZ { get; set; }
        public float? Heading { get; set; }
        public uint OwnerId { get; set; }

        public void SetPosition(Vector3 position)
        {
            PositionX = position.X;
            PositionY = position.Y;
            PositionZ = position.Z;
        }

        public Vector3 GetPosition()
        {
            if (PositionX.HasValue && PositionY.HasValue && PositionZ.HasValue)
            {
                return new Vector3(PositionX.Value, PositionY.Value, PositionZ.Value);
            }

            return null;
        }
    }

    public static class VehicleDatabase
    {
        public static async void InitializeTable()
        {
            string query = $@"CREATE TABLE IF NOT EXISTS `Vehicles` (
                            `id` int UNSIGNED AUTO_INCREMENT PRIMARY KEY NOT NULL,
                            `vehicle_name` varchar(255) NULL,
                            `color_1` int NOT NULL DEFAULT 112,
                            `color_2` int NOT NULL DEFAULT 112,
                            `position_x` varchar(255) NULL,
                            `position_y` varchar(255) NULL,
                            `position_z` varchar(255) NULL,
                            `heading` varchar(255) NULL,
                            `owner_id` int UNSIGNED NOT NULL,
                            FOREIGN KEY(owner_id) REFERENCES Characters(id)
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

        public static async Task<VehicleData> CreateVehicle(VehicleData vehicleData)
        {
            string query = $@"INSERT INTO Vehicles (`vehicle_name`, `owner_id`, `position_x`, `position_y`, `position_z`) VALUES ('{vehicleData.VehicleName}', {vehicleData.OwnerId}, {vehicleData.PositionX}, {vehicleData.PositionY}, {vehicleData.PositionZ});";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    vehicleData.Id = (uint)command.LastInsertedId;
                    return vehicleData;

                }
                catch (Exception e)
                {
                    Main.Logger.LogError(e.Message);
                }
            }

            return null;
        }


        public static async void SaveVehicle(VehicleData vehicleData)
        {
            string query = $@"UPDATE Vehicles SET 
                                `position_x` = '{vehicleData.PositionX}', 
                                `position_y` = '{vehicleData.PositionY}', 
                                `position_z` = '{vehicleData.PositionZ}',
                                `heading` = '{vehicleData.Heading}',
                                `color_1` = {vehicleData.Color1},
                                `color_2` = {vehicleData.Color2} WHERE id = {vehicleData.Id}";

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

        public static async Task<VehicleData> GetVehicleData(uint vehicleId)
        {

            string query = $"SELECT * FROM `vehicles` WHERE `id` = {vehicleId}";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            VehicleData vehicleData = new VehicleData()
                            {
                                Id = (uint)reader[0],
                                VehicleName = reader[1].ToString(),
                                Color1 = (int)reader[2],
                                Color2 = (int)reader[3],
                                PositionX = reader.FloatOrNull(4),
                                PositionY = reader.FloatOrNull(5),
                                PositionZ = reader.FloatOrNull(6),
                                Heading = reader.FloatOrNull(7),
                                OwnerId = uint.Parse(reader[8].ToString())
                            };
                            return vehicleData;
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

        public static async Task<uint[]> GetVehicles()
        {
            string query = $"SELECT * FROM `vehicles`";

            using (MySqlCommand command = new MySqlCommand(query, Main.Connection))
            {
                try
                {
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            List<uint> vehicleIds = new List<uint>();
                            while (reader.Read())
                            {
                                vehicleIds.Add((uint)reader[0]);
                            }
                            return vehicleIds.ToArray();
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
    }
}
