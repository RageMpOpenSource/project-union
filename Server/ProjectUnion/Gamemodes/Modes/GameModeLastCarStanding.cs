using GTANetworkAPI;
using ProjectUnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUnion.GameModes.Modes
{

    public class GameModeDataLastCarStanding : BaseGameModeData
    {
        public VehicleHash VehicleChosen { get; set; }
        public List<Vehicle> CurrentVehicles { get; set; } = new List<Vehicle>();

        public GameModeDataLastCarStanding()
        {
            base.Type = GameModeType.LastCarStanding;
        }
    }

    public class GameModeLastCarStanding : BaseGameMode
    {
        private GameModeDataLastCarStanding Data { get { return (GameModeDataLastCarStanding)GameModeData; } }
        public GameModeLastCarStanding(GameModeHandler gameModeHandler, uint id, Client host) : base(gameModeHandler, new GameModeDataLastCarStanding(), id, host, "Last Car Standing")
        {
        }

        protected override void OnAboutToStart()
        {
            Data.VehicleChosen = GetMapData().VehicleList[Main.Random.Next(GetMapData().VehicleList.Length)];
            SpawnVehicles();
        }

        protected override void OnAddPlayer(Client client)
        {
        }

        protected override void OnStart()
        {
            LogAllCurrentPlayers("Don't fall off the edge!");
            SpawnPlayersIntoVehicles();
        }

        private void SpawnPlayersIntoVehicles()
        {
            for (int i = 0; i < Data.CurrentPlayers.Count; i++)
            {
                NAPI.Player.SetPlayerIntoVehicle(Data.CurrentPlayers[i], Data.CurrentVehicles[i], -1);
                NAPI.Vehicle.BreakVehicleDoor(Data.CurrentVehicles[i], 0, true);
                NAPI.Vehicle.BreakVehicleWindow(Data.CurrentVehicles[i], 0, true);
                NAPI.Vehicle.BreakVehicleWindow(Data.CurrentVehicles[i], 1, true);
                NAPI.Vehicle.BreakVehicleWindow(Data.CurrentVehicles[i], 2, true);
                NAPI.Vehicle.BreakVehicleWindow(Data.CurrentVehicles[i], 3, true);
            }
        }

        protected override void OnStop()
        {
            DeleteVehicles();
        }

        private void DeleteVehicles()
        {
            foreach (var vehicle in Data.CurrentVehicles)
            {
                NAPI.Task.Run(() =>
                {
                    vehicle.Delete();
                });
            }

            Data.CurrentVehicles = new List<Vehicle>();
        }

        protected override void OnStartCountdown()
        {
            Data.VehicleChosen = GetMapData().VehicleList[Main.Random.Next(GetMapData().VehicleList.Length)];
        }

        private void SpawnVehicles()
        {
            List<GamePosition> vehicleStartPositions = GetMapData().VehicleStartPositions.ToList();

            for (int i = 0; i < Data.CurrentPlayers.Count; i++)
            {
                int randomIndex = Main.Random.Next(0, vehicleStartPositions.Count);
                GamePosition vehicleSpawnPoint = vehicleStartPositions[i];
                vehicleStartPositions.RemoveAt(randomIndex);
                NAPI.Task.Run(() =>
                {
                    Vehicle vehicle = NAPI.Vehicle.CreateVehicle(Data.VehicleChosen, vehicleSpawnPoint.GetPosition(), vehicleSpawnPoint.Heading, Main.Random.Next(0, 112), Main.Random.Next(0, 112), dimension: GameModeData.Id * 10);
                    Data.CurrentVehicles.Add(vehicle);
                });
            }
        }

        protected override void OnTick()
        {
            List<int> vehiclesToRemove = new List<int>();
            for (int i = 0; i < Data.CurrentVehicles.Count; i++)
            {
                Vehicle vehicle = Data.CurrentVehicles[i];
                if (vehicle.Position.Z <= GetMapData().DeathZ)
                {
                    NAPI.ClientEvent.TriggerClientEvent(Data.CurrentPlayers[i], "TriggerVehicleExplosion");
                    vehiclesToRemove.Add(i);
                    Data.CurrentPlayers[i].Health = 100;
                }
                else
                {
                    vehicle.Health = 100;
                }
            }

            for (int i = vehiclesToRemove.Count - 1; i >= 0; i--)
            {
                Data.CurrentVehicles.RemoveAt(vehiclesToRemove[i]);
            }
        }



        protected override void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            GamePosition spectatePoint = GetMapData().SpectateSpawnPoints[Main.Random.Next(GetMapData().SpectateSpawnPoints.Length)];
            ServerUtilities.SpawnPlayerAfter(client, spectatePoint);
        }
    }
}
