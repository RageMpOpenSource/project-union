﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkAPI;
using ProjectUnion.Data;

namespace ProjectUnion.Server
{
    public static class ServerUtilities
    {

        private static PlayerSpawnPoint _spawnPositions;

        private class PlayerSpawnPoint
        {

            public GamePosition[] Locations { get; set; }
        }

        public static void Initialise()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var json = File.ReadAllText(Path.Combine(currentDirectory, "PlayerSpawnPoints.json"));
            _spawnPositions = NAPI.Util.FromJson<PlayerSpawnPoint>(json);
        }


        public static uint GetPlayerLoginIndex()
        {
            List<Client> players = NAPI.Pools.GetAllPlayers();

            List<PlayerTempData> allPlayerTempData = players.Where(e => e.HasData(PlayerTempData.PLAYER_TEMP_DATA_KEY))
                                                    .Select(e => (PlayerTempData)e.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY)).ToList();

            uint smallestIndex = 1;
            for (int i = 0; i < allPlayerTempData.Count; i++)
            {
                if (allPlayerTempData[i].LoginIndex == smallestIndex)
                {
                    smallestIndex++;
                }
            }

            return smallestIndex;
        }

        public static async void SetPlayerNametag(Client client)
        {

            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_KEY);
            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);
            GroupData highestRankedGroup = await GroupDatabase.GetPlayerHighestRankingGroup(playerData.Id);



            if (characterData == null) return;
            client.Name = characterData.Name;

            if (highestRankedGroup == null)
            {
                client.Name = characterData.Name;
                NAPI.Player.SetPlayerName(client, characterData.Name);
                return;
            }

            var hexColor = highestRankedGroup.Color;

            client.Name = "[!{" + hexColor + "}" + highestRankedGroup.Name + "~w~] " + characterData.Name;
            NAPI.Player.SetPlayerName(client, "[!{" + hexColor + "}" + highestRankedGroup.Name + "~w~] " + characterData.Name);
        }


        public static void SwitchPlayerPosition(Client client, Vector3 pos, float heading)
        {
            uint tempModel = (uint)PedHash.AviSchwartzman;
            NAPI.ClientEvent.TriggerClientEvent(client, "StartPlayerSwitch", pos);

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 2500;
            aTimer.Enabled = true;
            void OnTimedEvent(object sender, EventArgs e)
            {
                NAPI.Player.SetPlayerSkin(client, tempModel);
                client.Position = pos;
                aTimer.Stop();
                aTimer.Dispose();
            };
        }

        public static async Task<bool> CanUseCommand(Client client, string command)
        {
            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_KEY);
            if (playerData == null)
            {
                client.SendChatMessage("You are not logged in! Please reconnect.");
                return false;
            }

            bool canUse = await GroupDatabase.DoesPlayerHaveCommand(playerData.Id, command);
            if (canUse == false)
            {
                Main.Logger.LogClient(client, "You do not have access to this command.");
                return false;
            }

            return true;
        }

        public static Client GetPlayerIfExists(Client client, string playerFirstName, string playerSurname = "")
        {
            var players = NAPI.Pools.GetAllPlayers();
            var playerNames = players.Select(e => e.Name);

            playerFirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerFirstName);

            int playersWithSimilarFirstName = 0;
            if (playerNames.Any(e => e.Contains(playerFirstName)))
            {
                playersWithSimilarFirstName++;
            }

            Client existingPlayer = null;

            if (playersWithSimilarFirstName == 1)
            {
                foreach (var player in players)
                {
                    if (player.Name.IndexOf(playerFirstName) > -1)
                    {
                        existingPlayer = player;
                    }
                }
            }
            else
            {
                if (playersWithSimilarFirstName > 1)
                {
                    if (string.IsNullOrEmpty(playerSurname) == false)
                    {
                        foreach (var player in players)
                        {
                            if (player.Name.IndexOf(playerFirstName) > -1 && player.Name.IndexOf(playerSurname) > -1)
                            {
                                existingPlayer = player;
                            }
                        }
                    }
                }
            }


            if (existingPlayer == null)
            {

                Main.Logger.LogClient(client, $"Player {playerFirstName} not found.");
                return null;
            }

            return existingPlayer;
        }


        public static void SpawnPlayerAfter(Client client, GamePosition position = null, int timeMs = 3000, Action callback = null)
        {

            System.Timers.Timer _spawnTimer = new System.Timers.Timer();
            _spawnTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _spawnTimer.Interval = timeMs;
            _spawnTimer.Enabled = true;

            void OnTimedEvent(object sender, EventArgs e)
            {
                SpawnPlayer(client, position, callback);
                _spawnTimer.Stop();
                _spawnTimer.Dispose();
            }
        }


        public static void SpawnPlayer(Client client, GamePosition positionToSpawn = null, Action callback = null)
        {
            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);


            if (positionToSpawn == null)
            {
                if (characterData.GetPosition() == null)
                {
                    positionToSpawn = GetRandomSpawnPoint();
                }
                else
                {
                    positionToSpawn = new GamePosition();
                    positionToSpawn.SetPosition(characterData.GetPosition());
                    positionToSpawn.Heading = characterData.Heading.HasValue ? characterData.Heading.Value : 0;
                }
            }

            NAPI.Player.SpawnPlayer(client, positionToSpawn.GetPosition(), positionToSpawn.GetHeading());
            if (callback != null)
            {
                callback();

            }
        }



        public static GamePosition GetRandomSpawnPoint()
        {
            GamePosition spawnPoint = _spawnPositions.Locations[Main.Random.Next(_spawnPositions.Locations.Length)];
            return spawnPoint;
        }

      
    }
}
