using GTANetworkAPI;
using ProjectUnion.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ProjectUnion.Events
{
    public class PlayerEvents : Script
    {
        //TODO : Create LoginEvents class?

        private readonly PlayerSpawnPoint spawnPositions;

        private class PlayerSpawnPoint
        {
            public class Vector4
            {
                public float X { get; set; }
                public float Y { get; set; }
                public float Z { get; set; }
                public float Heading { get; set; }

                public Vector3 GetPosition()
                {
                    return new Vector3(X, Y, Z);
                }
            }

            public Vector4[] Locations { get; set; }
        }

        public PlayerEvents()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var json = File.ReadAllText(Path.Combine(currentDirectory, "PlayerSpawnPoints.json"));
            spawnPositions = NAPI.Util.FromJson<PlayerSpawnPoint>(json);
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

            uint[] characters = await CharacterDatabase.GetCharacters(playerData.Id);
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
                characterData = await CharacterDatabase.GetCharacterData(characters.First());
            }


            client.SetData(PlayerData.PLAYER_DATA_KEY, playerData);
            client.SetData(CharacterData.CHARACTER_DATA_KEY, characterData);

            ShowCharacterSelectScreen(client);
        }

        private async void ShowCharacterSelectScreen(Client client)
        {
            PlayerData playerData = await Data.PlayerDatabase.GetPlayerData(client.Address);
            uint[] characters = await CharacterDatabase.GetCharacters(playerData.Id);
            CharacterData[] charactersData = await Task.WhenAll(characters.Select(async e => await CharacterDatabase.GetCharacterData(e)));
            string[] characterNames = charactersData.Select(e => e.Name).ToArray();
            NAPI.ClientEvent.TriggerClientEvent(client, "SelectCharacter", string.Join(",", characters), string.Join(",", characterNames));
            NAPI.Player.SetPlayerSkin(client, PedHash.MovAlien01);
        }


        [RemoteEvent("SelectCharacter")]
        public async void SelectCharacter(Client client, object[] args)
        {
            uint characterId = (uint)(int)args[0];
            CharacterData characterData = await CharacterDatabase.GetCharacterData(characterId);

            var position = characterData.GetPosition();
            float heading = 0;
            if (position == null)
            {
                var spawnPoint = GetRandomSpawnPoint();
                position = spawnPoint.GetPosition();
                heading = spawnPoint.Heading;
            }

            UpdatePlayerPed(client, position, heading);
        }

        private void UpdatePlayerPed(Client client, Vector3 pos, float heading)
        {
            uint tempModel = (uint)PedHash.AviSchwartzman;
            NAPI.ClientEvent.TriggerClientEvent(client, "StartPlayerSwitch", pos);

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 3000;
            aTimer.Enabled = true;
            void OnTimedEvent(object sender, EventArgs e)
            {
                NAPI.Player.SetPlayerSkin(client, tempModel);
                client.Position = pos;
                //NAPI.ClientEvent.TriggerClientEvent(client, "StartPlayerSwitch");
                aTimer.Stop();
                aTimer.Dispose();
            };
        }


        public void SpawnPlayer(Client client)
        {
            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (characterData.GetPosition() == null)
            {
                var spawnPoint = GetRandomSpawnPoint();
                NAPI.Player.SpawnPlayer(client, spawnPoint.GetPosition(), spawnPoint.Heading);
                NAPI.Chat.SendChatMessageToPlayer(client, "Spawned at random pos");
            }
            else
            {
                NAPI.Player.SpawnPlayer(client, characterData.GetPosition(), 0);
            }
        }

        private PlayerSpawnPoint.Vector4 GetRandomSpawnPoint()
        {
            var spawnPoint = spawnPositions.Locations[Main.Random.Next(spawnPositions.Locations.Length)];
            return spawnPoint;
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


        #region Login Events


        [RemoteEvent("CreateCharacter")]
        public async void CreateCharacter(Client client, object[] args)
        {
            string name = args[0].ToString();

            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_KEY);
            CharacterData characterData = new CharacterData()
            {
                OwnerId = playerData.Id,
                Name = name
            };

            await Data.CharacterDatabase.CreateCharacter(characterData);
            //client.SetData(CharacterData.CHARACTER_DATA_KEY, characterData);
            //var spawnPoint = GetRandomSpawnPoint();
            //UpdatePlayerPed(client, spawnPoint.GetPosition(), spawnPoint.Heading);
            ShowCharacterSelectScreen(client);
        }



        [RemoteEvent("GoBackToCharacterSelection")]
        public async void OnGoBackToCharacterSelect(Client client)
        {
            PlayerData playerData = await Data.PlayerDatabase.GetPlayerData(client.Address);

            uint[] characters = await CharacterDatabase.GetCharacters(playerData.Id);
            CharacterData[] charactersData = await Task.WhenAll(characters.Select(async e => await CharacterDatabase.GetCharacterData(e)));
            string[] characterNames = charactersData.Select(e => e.Name).ToArray();

            NAPI.ClientEvent.TriggerClientEvent(client, "ToggleCreateCharacterMenu", false);
            NAPI.ClientEvent.TriggerClientEvent(client, "SelectCharacter", string.Join(",", characters), string.Join(",", characterNames));
        }

        #endregion
    }
}
