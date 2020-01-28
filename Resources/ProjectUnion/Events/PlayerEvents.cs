using GTANetworkAPI;
using ProjectUnion.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;

namespace ProjectUnion.Events
{
    public class PlayerEvents : Script
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
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public PlayerEvents()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var json = File.ReadAllText(Path.Combine(currentDirectory, "PlayerSpawnPoints.json"));
            spawnPositions = NAPI.Util.FromJson<PlayerSpawnPoints>(json);
        }

        [ServerEvent(Event.PlayerConnected)]
        public async void OnPlayerConnected(Client client)
        {
            PlayerData playerData = null;
            CharacterData characterData = null;

            playerData = await Data.PlayerDatabase.GetPlayerData(client.Address);

            if (playerData != null)
            {
                Main.Logger.Log($"Last login was at {playerData.LastLogin.ToString()}");
            }

            if (playerData == null)
            {

                playerData = new PlayerData()
                {
                    PlayerHash = client.Address,
                    LastLogin = DateTime.Now
                };

                playerData = await Data.PlayerDatabase.CreatePlayer(playerData);

                characterData = new CharacterData()
                {
                    OwnerId = playerData.Id
                };

                characterData = await Data.CharacterDatabase.CreateCharacter(characterData);
            }
            else
            {
                uint[] characters = await CharacterDatabase.GetCharacters(playerData.Id);
                characterData = await CharacterDatabase.GetCharacterData(characters.First());
            }


            client.SetData(PlayerData.PLAYER_DATA_KEY, playerData);
            client.SetData(CharacterData.CHARACTER_DATA_KEY, characterData);

            SpawnPlayer(client);
        }


        public void SpawnPlayer(Client client)
        {
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);

            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (characterData.GetPosition() == null)
            {
                var spawnPoint = spawnPositions.Locations[Main.Random.Next(spawnPositions.Locations.Length)];
                var pos = new Vector3(spawnPoint.X, spawnPoint.Y, spawnPoint.Z);
                NAPI.Player.SpawnPlayer(client, pos, spawnPoint.Heading);
                NAPI.Chat.SendChatMessageToPlayer(client, "Spawned at random pos");
            }
            else
            {
                NAPI.Player.SpawnPlayer(client, characterData.GetPosition(), 0);
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;

            void OnTimedEvent(object sender, EventArgs e)
            {
                //Call method
                SpawnPlayer(client);
                aTimer.Stop();
                aTimer.Dispose();
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Client player, DisconnectionType type, string reason)
        {
            PlayerData playerData = player.GetData(PlayerData.PLAYER_DATA_KEY);
            playerData.LastLogin = DateTime.Now;
            PlayerDatabase.SavePlayer(playerData);

            CharacterData characterData = player.GetData(CharacterData.CHARACTER_DATA_KEY);
            characterData.PositionX = player.Position.X;
            characterData.PositionY = player.Position.Y;
            characterData.PositionZ = player.Position.Z;
            CharacterDatabase.SaveCharacter(characterData);
        }
    }
}
