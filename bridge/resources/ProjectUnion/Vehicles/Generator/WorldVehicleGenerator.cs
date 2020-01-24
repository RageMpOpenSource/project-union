using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectUnion.Vehicles
{
    public class WorldVehicleGenerator : Script
    {
        private class VehicleSpawnData
        {
            public float X;
            public float Y;
            public float Z;
            public float Heading;
            public string[] Models;
        }

        private readonly VehicleSpawnData[] allVehicleSpawnData;

        public WorldVehicleGenerator()
        {
            var jsonData = File.ReadAllText("./bridge/resources/ProjectUnion/Vehicles/Generator/VehicleGenerationData.json");
            allVehicleSpawnData = NAPI.Util.FromJson<VehicleSpawnData[]>(jsonData);
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            GenerateVehicles();
        }

        private void GenerateVehicles()
        {
            var totalCarsToSpawn = 1200;
            var chanceToSpawn = 85;
            var totalSpawnOpportunities = allVehicleSpawnData.Length;
            var totalSpawned = 0;

            for (int i = 0; i < totalCarsToSpawn; i++)
            {
                var spawnData = allVehicleSpawnData[Main.Random.Next(totalSpawnOpportunities)];
                if (Main.Random.Next(100) > chanceToSpawn || spawnData.Models.Length == 0) continue;
                var randomModel = spawnData.Models[Main.Random.Next(spawnData.Models.Length)];
                var modelHash = NAPI.Util.GetHashKey(randomModel);
                NAPI.Vehicle.CreateVehicle(modelHash, new Vector3(spawnData.X, spawnData.Y, spawnData.Z), spawnData.Heading, Main.Random.Next(115), Main.Random.Next(115));
                totalSpawned++;
            }

            Main.Logger.Log($"Random Vehicle Generator: Spawned {totalSpawned} out of {totalSpawnOpportunities}");
        }

    }
}
