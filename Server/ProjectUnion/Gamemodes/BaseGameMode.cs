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
        public bool IsGameModeDestroyed { get; internal set; }
        public bool IsGameModeFinished { get; internal set; }
        private bool DidGameModeBegin { get; set; }


        protected abstract void OnPrepare();
        protected abstract void OnStart();
        protected abstract void OnDestroy();
        protected abstract void OnTick();
        protected abstract void OnPlayerDeath(Client client, Client killer, uint reason);
        protected abstract void OnAddPlayer(Client client);
        protected abstract void OnRemovePlayer(Client client);
        protected abstract void OnSetPlayerState(Client client, int clientIndex);
        protected abstract void OnResetPlayerState(Client client, int clientIndex);
        protected abstract void OnFinished();

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
                throw new Exception($"GameMode countdown started, please stop gamemode with /{Commands.COMMAND_GAMEMODE_STOP_COUNTDOWN} before trying to change any parameters.");
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
            }, null, timeTillGameModeStartsInMs, System.Threading.Timeout.Infinite);

            Main.Logger.LogClient(GameModeData.EventHost, $"Game Mode {GameModeData.Id}'s countdown has begun.");
        }


        private void StopCountdown()
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

            NAPI.Task.Run(() =>
            {

                OnPrepare();

                for (int clientIndex = 0; clientIndex < GameModeData.CurrentPlayers.Count; clientIndex++)
                {
                    Client client = GameModeData.CurrentPlayers[clientIndex];
                    PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);
                    playerTempData.GamemodeId = GameModeData.Id;
                    client.SetData(PlayerTempData.PLAYER_TEMP_DATA_KEY, playerTempData);

                    OnSetPlayerState(client, clientIndex);
                }

                _isStarted = true;
                _tickTimer = new System.Threading.Timer(Tick, null, TickTime, System.Threading.Timeout.Infinite);
                Main.Logger.LogClient(GameModeData.EventHost, $"Game Mode {GameModeData.Id} has begun.");
                DidGameModeBegin = true;

                OnStart();

            });
        }

        protected void FinishGameMode()
        {
            if (IsGameModeFinished == false)
            {
                DestroyGameMode();
                OnFinished();
                IsGameModeFinished = true;
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

        protected void SavePlayersPosition()
        {
            for (int i = 0; i < GameModeData.CurrentPlayers.Count; i++)
            {
                Client player = GameModeData.CurrentPlayers[i];

                GamePosition gamePosition = new GamePosition();
                gamePosition.SetPosition(player.Position);
                gamePosition.Heading = player.Heading;
                GameModeData.PlayerPositions.Insert(i, gamePosition);
            }
        }

        protected void TeleportPlayersIn()
        {
            SavePlayersPosition();
            for (int i = 0; i < GameModeData.CurrentPlayers.Count; i++)
            {
                Client player = GameModeData.CurrentPlayers[i];


                GamePosition randomSpawn = GetRandomSpawnPosition();
                ServerUtilities.SpawnPlayer(player, randomSpawn);
            }
        }

        private void TeleportPlayersOut()
        {
            if (DidGameModeBegin)
            {
                for (int i = 0; i < GameModeData.CurrentPlayers.Count; i++)
                {
                    TeleportPlayerOut(GameModeData.CurrentPlayers[i]);
                }
            }
        }


        private void TeleportPlayerOut(Client client)
        {
            int clientIndex = GameModeData.CurrentPlayers.IndexOf(client);

            if (clientIndex > -1)
            {
                GamePosition position = GameModeData.PlayerPositions[clientIndex];
                ServerUtilities.SpawnPlayer(client, position);

                PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);
                playerTempData.GamemodeId = null;
                client.SetData(PlayerTempData.PLAYER_TEMP_DATA_KEY, playerTempData);
                OnResetPlayerState(client, clientIndex);
            }

        }



        protected GamePosition GetRandomSpawnPosition()
        {
            BaseGameModeMapData mapData = GetMapData();
            return mapData.SpawnPoints[Main.Random.Next(mapData.SpawnPoints.Length)];
        }

        public virtual void StopGameModeCountdown()
        {
            if (_countdownTimer == null)
            {
                throw new Exception("Game mode countdown not started.");
            }

            StopCountdown();
        }

        public void DestroyGameMode()
        {

            if (IsGameModeDestroyed)
            {
                throw new Exception("Game Mode already destroyed.");
            }

            StopCountdown();
            IsGameModeDestroyed = true;

            System.Timers.Timer _stopTimer = new System.Timers.Timer();
            _stopTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _stopTimer.Interval = 5000;
            _stopTimer.Enabled = true;

            void OnTimedEvent(object sender, EventArgs e)
            {
                StopCountdown();
                TeleportPlayersOut();
                OnDestroy();

                _stopTimer.Dispose();
                _stopTimer.Stop();
                _gameModeHandler.DestroyGameMode(this);
            }
        }



        public BaseGameModeData GetGameModeData()
        {
            return GameModeData;
        }



        public void OnDeath(Client player, Client killer, uint reason)
        {
            if (_isStarted && IsPlayerInGameMode(player))
            {
                OnPlayerDeath(player, killer, reason);
            }
        }

        private bool IsPlayerInGameMode(Client client)
        {
            return GetGameModeData().CurrentPlayers.IndexOf(client) > -1;
        }

        public void AddPlayer(Client client)
        {
            if (GameModeData.CurrentPlayers.Contains(client))
            {
                throw new Exception($"Player already in the event ({GetGameModeData().Id}) {GetGameModeData().Name}.");
            }

            if (DidGameModeBegin)
            {
                throw new Exception($"Gamemode already started!");
            }

            GameModeData.CurrentPlayers.Add(client);
            OnAddPlayer(client);
        }
        public void RemovePlayer(Client client)
        {
            if (GameModeData.CurrentPlayers.Contains(client) == false)
            {
                throw new Exception($"Player is not in the event ({GetGameModeData().Id}) {GetGameModeData().Name}.");
            }


            OnRemovePlayer(client);

            if (DidGameModeBegin)
            {
                TeleportPlayerOut(client);
            }

            GameModeData.CurrentPlayers.Remove(client);
        }

    }
}
