using GTANetworkAPI;
using ProjectUnion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Server
{
    public class VehicleRespawner : Script
    {

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            RespawnVehicles();

            int timeBeforeRespawnToAnnounceInMinutes = 15;
            int lastHour = DateTime.Now.Hour;
            bool isRespawnAnnounced = false;


            var checkTime = new System.Threading.Timer((e) =>
            {
                if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
                {
                    isRespawnAnnounced = false;
                    lastHour = DateTime.Now.Hour;
                    RespawnVehicles();
                }
                else
                {
                    if (isRespawnAnnounced == false && DateTime.Now.Minute == (60 - timeBeforeRespawnToAnnounceInMinutes))
                    {
                        NAPI.Chat.SendChatMessageToAll($"Vehicles will be respawned in {(60 - timeBeforeRespawnToAnnounceInMinutes)} minutes.");
                        NAPI.Chat.SendChatMessageToAll($"Make sure you / park to save your vehicle's position. (or get in it to avoid respawn)");
                        isRespawnAnnounced = true;
                    }
                }


            }, null, 0, 1000);
        }

        public static async void RespawnVehicles()
        {

            List<Vehicle> vehiclesToDelete = NAPI.Pools.GetAllVehicles();

            List<Client> allPlayers = NAPI.Pools.GetAllPlayers();
            List<uint> vehiclesToNotRespawn = new List<uint>();

            foreach (var player in allPlayers)
            {
                var vehicle = NAPI.Player.GetPlayerVehicle(player);
                if (vehicle == null) continue;
                vehiclesToDelete.Remove(vehicle);
                var vehicleData = vehicle.GetData(VehicleData.VEHICLE_DATA_KEY);
                if (vehicleData == null) continue;
                vehiclesToNotRespawn.Add(vehicleData.Id);
            }

            //Delete vehicles
            foreach (Vehicle vehicle in vehiclesToDelete)
            {
                NAPI.Task.Run(() =>
                {
                    vehicle.Delete();
                });
            }

            uint[] vehicleIds = await VehicleDatabase.GetVehicles();
            VehicleData[] vehiclesToSpawn = await Task.WhenAll(vehicleIds.Where(e => vehiclesToNotRespawn.Contains(e) == false)
                                                                         .Select(e => VehicleDatabase.GetVehicleData(e)));

            foreach (VehicleData vehicleData in vehiclesToSpawn)
            {
                uint vehicleHash = NAPI.Util.GetHashKey(vehicleData.VehicleName);
                float heading = 0;
                if (vehicleData.Heading.HasValue)
                {
                    heading = vehicleData.Heading.Value;
                }

                NAPI.Task.Run(() =>
                {
                    var vehicle = NAPI.Vehicle.CreateVehicle(vehicleHash, vehicleData.GetPosition(), heading, vehicleData.Color1, vehicleData.Color2);
                    vehicle.SetData(VehicleData.VEHICLE_DATA_KEY, vehicleData);
                });
            }

            NAPI.Chat.SendChatMessageToAll($"All vehicles respawned");
        }
    }
}
