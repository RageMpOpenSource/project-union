using GTANetworkAPI;
using ProjectUnion.GameModes.Modes;
using ProjectUnion.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProjectUnion.GameModes
{

    public class MapJsonData
    {
        public BaseGameModeMapData[] Maps { get; set; }
    }


    public class GameModeHandler
    {
        private static GameModeHandler _instance;
        public static GameModeHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameModeHandler();
                }
                return _instance;
            }
        }

        private List<BaseGameMode> _gameModes = new List<BaseGameMode>();
        private BaseGameModeMapData[] _maps;

        public uint GameModeIndex = 0;

        public GameModeHandler()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string json = File.ReadAllText(Path.Combine(currentDirectory, "GameModeMaps.json"));
            MapJsonData mapData = NAPI.Util.FromJson<MapJsonData>(json);
            _maps = mapData.Maps;

        }


        public uint CreateGameMode(Client host, GameModeType gameModeType)
        {
            BaseGameMode gameMode = null;

            switch (gameModeType)
            {
                case GameModeType.Deathmatch:
                    gameMode = new GameModeDeathmatch(this, GameModeIndex, host);
                    break;
                case GameModeType.LastCarStanding:
                    gameMode = new GameModeLastCarStanding(this, GameModeIndex, host);
                    break;
                default:
                    throw new Exception("Game Type not found!");
            }

            _gameModes.Add(gameMode);
            GameModeIndex++;
            return gameMode.GetGameModeData().Id;
        }

        public BaseGameMode GetGameModeById(uint id)
        {
            return _gameModes.Single(e => e.GetGameModeData().Id == id);
        }

        public List<BaseGameMode> GetGameModeByHost(Client host)
        {
            return _gameModes.Where(e => e.GetGameModeData().EventHost == host).ToList();
        }


        public List<BaseGameModeMapData> GetSuitableMaps(GameModeType gameModeType)
        {
            return _maps.Where(e => e.SuitedGameModes.Contains(gameModeType)).ToList();
        }
        public BaseGameModeMapData GetMap(uint mapId)
        {
            return _maps.Single(e => e.MapId == mapId);
        }


        public void OnDeath(Client player, Client killer, uint reason)
        {
            foreach (BaseGameMode gameMode in _gameModes)
            {
                gameMode.OnDeath(player, killer, reason);
            }
        }
    }
}
