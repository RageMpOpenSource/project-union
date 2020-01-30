using GTANetworkAPI;
using ProjectUnion.Data;
using ProjectUnion.Server;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }

    public class BaseGameModeMapData
    {
        public uint MapId { get; set; }
        public string DisplayName { get; set; }
        public string EnumName { get; set; }
        public GameModeType[] SuitedGameModes { get; set; }

        public GamePosition[] SpawnPoints;
    }
    #endregion



    public abstract class BaseGameMode
    {
        private BaseGameModeData _gameModeData = null;

        public BaseGameMode(BaseGameModeData data, uint id, Client host, string name)
        {
            _gameModeData = data;
            _gameModeData.Id = id;
            _gameModeData.EventHost = host;
            _gameModeData.Name = name;
        }

        public void SetGameModeMapId(uint mapId)
        {

            BaseGameModeMapData mapData = Main.GameModeHandler.GetMap(mapId);
            if (mapData == null)
            {
                throw new System.Exception("Map not found!");
            }

            if (mapData.SuitedGameModes.Contains(_gameModeData.Type) == false)
            {
                throw new System.Exception("Map is not suited to this game mode!");
            }

            _gameModeData.MapId = mapId;

            if (mapData.SpawnPoints.Length < _gameModeData.MaxPlayers)
            {
                Main.Logger.LogClient(_gameModeData.EventHost, $"Map does not support number of players {_gameModeData.MaxPlayers}, lowering to {mapData.SpawnPoints.Length}.");
                _gameModeData.MaxPlayers = (uint)mapData.SpawnPoints.Length;
            }
        }

        public void SetMaxPlayers(uint maxPlayers)
        {
            try
            {
                VerifyIsMapSet();

                BaseGameModeMapData mapData = Main.GameModeHandler.GetMap(_gameModeData.MapId.Value);
                if (maxPlayers > mapData.SpawnPoints.Length)
                {
                    throw new Exception("Map does not support that amount of players!");
                }

                _gameModeData.MaxPlayers = maxPlayers;
                Main.Logger.LogClient(_gameModeData.EventHost, $"Max players set to {maxPlayers}");

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void VerifyIsMapSet()
        {
            if (_gameModeData.MapId.HasValue == false)
            {
                throw new Exception($"Map Id not set. Set it using /{Commands.COMMAND_GAMEMODE_SET_MAP}.");
            }

            BaseGameModeMapData mapData = Main.GameModeHandler.GetMap(_gameModeData.MapId.Value);
            if (mapData == null)
            {
                throw new Exception($"Map Id not found. Verify game map json data required. Contact owner!");
            }
        }

        public BaseGameModeData GetGameModeData()
        {
            return _gameModeData;
        }

        public abstract void OnAddPlayer(Client client);

        public void AddPlayer(Client client)
        {
            PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);

            if (playerTempData.GamemodeId.HasValue)
            {
                BaseGameMode baseGameMode = Main.GameModeHandler.GetGameModeById(playerTempData.GamemodeId.Value);
                throw new Exception($"You are already in the event ({baseGameMode.GetGameModeData().Id}) {baseGameMode.GetGameModeData().Name}.");
            }

            OnAddPlayer(client);
        }

    }
}
