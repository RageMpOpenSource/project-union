using GTANetworkAPI;
using ProjectUnion.Data;
using ProjectUnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ProjectUnion.GameModes
{

    #region Data
    public enum GameModeType
    {
        Deathmatch,
        LastCarStanding
    }

    public class BaseGameModeData
    {
        public Client EventHost;
        public uint Id { get; set; }
        public string Name { get; set; } = "Default Game Mode";
        public uint MaxPlayers { get; set; } = 8;

        public GameModeType Type { get; set; }

        public List<Client> CurrentPlayers { get; set; } = new List<Client>();
        public uint? MapId { get; set; }

        public List<GamePosition> PlayerPositions { get; set; } = new List<GamePosition>(100);
    }

    public class BaseGameModeMapData
    {
        public uint MapId { get; set; }
        public string DisplayName { get; set; }
        public string EnumName { get; set; }
        public int MaxPlayers { get; set; }
        public GameModeType[] SuitedGameModes { get; set; }

        public GamePosition[] SpawnPoints { get; set; }

        //Last Car Standing
        public VehicleHash[] VehicleList { get; set; }
        public GamePosition[] VehicleStartPositions { get; set; }
        public float DeathZ { get; set; }
        public GamePosition[] SpectateSpawnPoints { get; set; }

        //Deathmatch
        public WeaponHash[] WeaponList { get; set; }

    }
    #endregion



    public abstract class BaseGameMode
    {
        private readonly GameModeHandler _gameModeHandler;

        private int _announceTimeBeforeStartInMs = 7 * 1000;
        private int _totalTimeTillGameModeStartsInMs = 15 * 1000;
        private System.Threading.Timer _countdownTimer;
        private System.Threading.Timer _announceTimer;
        private System.Threading.Timer _tickTimer;
        public int TickTime = 1000;

        protected BaseGameModeData GameModeData = null;
        private bool _isStarted;


        protected abstract void OnAddPlayer(Client client);
        protected abstract void OnAboutToStart();
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract void OnStartCountdown();
        protected abstract void OnTick();
        protected abstract void OnPlayerDeath(Client client, Client killer, uint reason);

        public BaseGameMode(GameModeHandler gameModeHandler, BaseGameModeData data, uint id, Client host, string name)
        {
            _gameModeHandler = gameModeHandler;

            GameModeData = data;
            GameModeData.Id = id;
            GameModeData.EventHost = host;
            GameModeData.Name = name;
            GameModeData.Type = data.Type;
        }

        private void VerifyCanChangeParameters()
        {
            if (_countdownTimer != null)
            {
                throw new Exception($"GameMode countdown started, please stop gamemode with /{Commands.COMMAND_GAMEMODE_STOP} before trying to change any parameters.");
            }
        }

        private void VerifyGameModeValid()
        {
            VerifyMapValid();
        }

        protected BaseGameModeMapData GetMapData()
        {
            VerifyMapValid();
            return _gameModeHandler.GetMap(GetGameModeData().MapId.Value);
        }

        public void SetGameModeMapId(uint mapId)
        {
            BaseGameModeMapData mapData = _gameModeHandler.GetMap(mapId);
            if (mapData == null)
            {
                throw new System.Exception("Map not found!");
            }

            if (mapData.SuitedGameModes.Contains(GameModeData.Type) == false)
            {
                throw new System.Exception("Map is not suited to this game mode!");
            }

            GameModeData.MapId = mapId;

            if (mapData.MaxPlayers < GameModeData.MaxPlayers)
            {
                Main.Logger.LogClient(GameModeData.EventHost, $"Map does not support number of players {GameModeData.MaxPlayers}, lowering to {mapData.MaxPlayers}.");
                GameModeData.MaxPlayers = (uint)mapData.SpawnPoints.Length;
            }

            Main.Logger.LogClient(GameModeData.EventHost, $"Map changed to ({mapData.MapId})  {mapData.DisplayName}.");
        }

        public void SetMaxPlayers(uint maxPlayers)
        {
            try
            {
                VerifyMapValid();

                BaseGameModeMapData mapData = GetMapData();
                if (maxPlayers > mapData.MaxPlayers)
                {
                    throw new Exception($"Map does not support that amount of players! Max Players Supported :{mapData.MaxPlayers}");
                }

                GameModeData.MaxPlayers = maxPlayers;
                Main.Logger.LogClient(GameModeData.EventHost, $"Max players set to {maxPlayers}");

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void VerifyMapValid()
        {
            if (GameModeData.MapId.HasValue == false)
            {
                throw new Exception($"Map Id not set. Set it using /{Commands.COMMAND_GAMEMODE_SET_MAP}.");
            }

            BaseGameModeMapData mapData = _gameModeHandler.GetMap(GetGameModeData().MapId.Value);
            if (mapData == null)
            {
                throw new Exception($"Map Id not found. Verify game map json data required. Contact owner!");
            }
        }

        public void StartCountdown()
        {
            StopCountdown();
            VerifyGameModeValid();

            int timeTillGameModeStartsInMs = (_totalTimeTillGameModeStartsInMs - _announceTimeBeforeStartInMs);
            _countdownTimer = new System.Threading.Timer((e) => { StartGameMode(); }, null, _totalTimeTillGameModeStartsInMs, System.Threading.Timeout.Infinite);
            _announceTimer = new System.Threading.Timer((e) =>
            {
                AnnounceAboutToStart(timeTillGameModeStartsInMs / 1000);
                OnAboutToStart();
            }, null, timeTillGameModeStartsInMs, System.Threading.Timeout.Infinite);

            OnStartCountdown();

            Main.Logger.LogClient(GameModeData.EventHost, $"Game Mode {GameModeData.Id}'s countdown has begun.");
        }


        public void StopCountdown()
        {
            if (_countdownTimer != null)
            {
                _countdownTimer.Dispose();
                _countdownTimer = null;
            }
            if (_announceTimer != null)
            {
                _announceTimer.Dispose();
                _announceTimer = null;
            }

        }


        protected void LogAllCurrentPlayers(string message)
        {
            foreach (var player in GameModeData.CurrentPlayers)
            {
                Main.Logger.LogClient(player, message);
            }
        }

        public void AnnounceAboutToStart(int secondsTillBegin)
        {
            LogAllCurrentPlayers($"Game Mode starting in {secondsTillBegin} seconds");
        }

        public virtual void StartGameMode()
        {
            StopCountdown();
            VerifyGameModeValid();
            _isStarted = true;
            Main.Logger.LogClient(GameModeData.EventHost, $"Game Mode {GameModeData.Id} has begun.");
            _tickTimer = new System.Threading.Timer(Tick, null, TickTime, System.Threading.Timeout.Infinite);
            OnStart();


            foreach (Client client in GameModeData.CurrentPlayers)
            {
                PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);
                playerTempData.GamemodeId = GameModeData.Id;
                client.SetData(PlayerTempData.PLAYER_TEMP_DATA_KEY, playerTempData);
            }

        }


        protected void Tick(object state)
        {
            _tickTimer.Change(TickTime, System.Threading.Timeout.Infinite);
            if (_isStarted)
            {
                OnTick();
            }
        }

        protected void TeleportPlayersIn()
        {
            for (int i = 0; i < GameModeData.CurrentPlayers.Count; i++)
            {
                Client player = GameModeData.CurrentPlayers[i];

                GamePosition gamePosition = new GamePosition();
                gamePosition.SetPosition(player.Position);
                gamePosition.Heading = player.Heading;
                GameModeData.PlayerPositions.Insert(i, gamePosition);
                GamePosition randomSpawn = GetRandomSpawnPosition();
                ServerUtilities.SpawnPlayer(player, randomSpawn);
            }
        }

        protected void TeleportPlayersOut()
        {
            for (int i = 0; i < GameModeData.CurrentPlayers.Count; i++)
            {
                Client client = GameModeData.CurrentPlayers[i];
                GamePosition position = GameModeData.PlayerPositions[i];
                ServerUtilities.SpawnPlayer(client, position);

                PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);
                playerTempData.GamemodeId = null;
                client.SetData(PlayerTempData.PLAYER_TEMP_DATA_KEY, playerTempData);

            }

        }


        protected GamePosition GetRandomSpawnPosition()
        {
            BaseGameModeMapData mapData = GetMapData();
            return mapData.SpawnPoints[Main.Random.Next(mapData.SpawnPoints.Length)];
        }

        public virtual void StopGameMode()
        {
            if (_isStarted == false && _countdownTimer == null)
            {
                throw new Exception("Game mode not started.");
            }
            _isStarted = false;

            System.Timers.Timer _spawnTimer = new System.Timers.Timer();
            _spawnTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _spawnTimer.Interval = 5000;
            _spawnTimer.Enabled = true;

            void OnTimedEvent(object sender, EventArgs e)
            {
                StopCountdown();
                OnStop();

                _spawnTimer.Dispose();
                _spawnTimer.Stop();
            }
        }



        public BaseGameModeData GetGameModeData()
        {
            return GameModeData;
        }


        public void AddPlayer(Client client)
        {
            PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);

            if (playerTempData.GamemodeId.HasValue)
            {
                BaseGameMode baseGameMode = _gameModeHandler.GetGameModeById(playerTempData.GamemodeId.Value);
                throw new Exception($"You are already in the event ({baseGameMode.GetGameModeData().Id}) {baseGameMode.GetGameModeData().Name}.");
            }

            GameModeData.CurrentPlayers.Add(client);

            OnAddPlayer(client);
        }


        public void OnDeath(Client player, Client killer, uint reason)
        {
            if (_isStarted)
            {
                OnPlayerDeath(player, killer, reason);
            }
        }

    }
}
