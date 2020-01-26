using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System;
using System.IO;

namespace ProjectUnion.Player.Events.Spawning
{
    public class Events : Script
    {
        private readonly PlayerSpawnPoints spawnPositions;

        private class PlayerSpawnPoints
        {
            public class Vector4
            {
                public float X { get; set; }
                public float Y { get; set; }
                public float Z { get; set; }
                public float Heading { get; set; }
            }

            public Vector4[] Locations { get; set; }
        }

        public Events()
        {
            var json = File.ReadAllText("./bridge/resources/ProjectUnion/Player/Events/PlayerSpawnPoints.json");
            spawnPositions = NAPI.Util.FromJson<PlayerSpawnPoints>(json);
        }


        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetAutoRespawnAfterDeath(false);
        }


        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            var spawnPoint = spawnPositions.Locations[Main.Random.Next(spawnPositions.Locations.Length)];
            var pos = new Vector3(spawnPoint.X, spawnPoint.Y, spawnPoint.Z);
            NAPI.Player.SpawnPlayer(client, pos, spawnPoint.Heading);
        }
    }
}
