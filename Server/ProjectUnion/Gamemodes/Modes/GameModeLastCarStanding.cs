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


        protected override void OnAddPlayer(Client client)
        {
        }

        protected override void OnRemovePlayer(Client client)
        {
        }

        protected override void OnPrepare()
        {
            Data.VehicleChosen = GetMapData().VehicleList[Main.Random.Next(GetMapData().VehicleList.Length)];
            SavePlayersPosition();
            SpawnVehicles();
        }

        protected override void OnSetPlayerState(Client client, int clientIndex)
        {
            SpawnPlayersIntoVehicles();
        }

        protected override void OnResetPlayerState(Client client, int clientIndex)
        {
        }


        protected override void OnStart()
        {
            LogAllCurrentPlayers("Don't fall off the edge!");
        }

        private void SpawnPlayersIntoVehicles()
        {
            for (int i = 0; i < Data.CurrentPlayers.Count; i++)
            {
                NAPI.Player.SetPlayerIntoVehicle(Data.CurrentPlayers[i], Data.CurrentVehicles[i], -1);
            }
        }

        protected override void OnDestroy()
        {
            DeleteVehicles();
        }

        private void DeleteVehicles()
        {
            foreach (var vehicle in Data.CurrentVehicles)
            {
                NAPI.Task.Run(() =>
                {
                    if (vehicle != null)
                        vehicle.Delete();
                });
            }

            Data.CurrentVehicles = new List<Vehicle>();
        }


        private void SpawnVehicles()
        {
            List<GamePosition> vehicleStartPositions = GetMapData().VehicleStartPositions.ToList();

            for (int i = 0; i < Data.CurrentPlayers.Count; i++)
            {
                int randomIndex = Main.Random.Next(0, vehicleStartPositions.Count);
                GamePosition vehicleSpawnPoint = vehicleStartPositions[i];
                vehicleStartPositions.RemoveAt(randomIndex);
                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(Data.VehicleChosen, vehicleSpawnPoint.GetPosition(), vehicleSpawnPoint.Heading, Main.Random.Next(0, 112), Main.Random.Next(0, 112), dimension: GameModeData.Id * 10);
                Data.CurrentVehicles.Add(vehicle);
            }
        }

        protected override void OnTick()
        {
            List<int> vehiclesToRemove = new List<int>();
            for (int i = 0; i < Data.CurrentVehicles.Count; i++)
            {
                Vehicle vehicle = Data.CurrentVehicles[i];
                if (vehicle != null)
                {
                    if (vehicle.Position.Z <= GetMapData().DeathZ)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(Data.CurrentPlayers[i], "TriggerVehicleExplosion");
                        Data.CurrentVehicles[i] = null;
                    }

                    if (vehicle.EngineStatus == false)
                    {
                        Data.CurrentVehicles[i] = null;
                    }
                }
            }

            if (IsGameModeFinished == false)
            {
                List<Vehicle> vehiclesLeft = Data.CurrentVehicles.Where(e => e != null).ToList();
                if (vehiclesLeft.Count == 0)
                {
                    for (int i = 0; i < vehiclesLeft.Count; i++)
                    {
                        if (vehiclesLeft[i] != null)
                        {
                            LogAllCurrentPlayers($"{Data.CurrentPlayers[i].Name} wins!");
                            FinishGameMode();
                        }
                    }
                }
            }
        }



        protected override void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            GamePosition spectatePoint = GetMapData().SpectateSpawnPoints[Main.Random.Next(GetMapData().SpectateSpawnPoints.Length)];
            ServerUtilities.SpawnPlayerAfter(client, spectatePoint);
        }

        protected override void OnFinished()
        {
        }
    }
}
