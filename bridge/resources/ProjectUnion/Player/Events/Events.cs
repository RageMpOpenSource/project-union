using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System.IO;
using System.Linq;

namespace ProjectUnion.Player.Events
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



        [ServerEvent(Event.PlayerConnected)]
        public async void OnPlayerConnected(Client client)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            if (playerData == null)
            {
                playerData = await PlayerData.CreatePlayerData(client);
                await PlayerGroups.PlayerGroupDatabase.AddPlayerToGroup(ProjectUnion.Database.MySQL.connection, playerData.Id, Config.GroupConfig.GROUP_NAME_PLAYER);
                NAPI.Util.ConsoleOutput("New Player Created!");
            }

            var allCharData = await CharacterData.GetAllCharacters(playerData.Id);
            var characterIds = allCharData.Select(e => e.Id).ToArray();
            var characterNames = allCharData.Select(e => e.Name).ToArray();

            client.SetData(PlayerData.PLAYER_DATA_ID, playerData);
            NAPI.ClientEvent.TriggerClientEvent(client, "ShowCharacterSelectMenu", string.Join(",", characterIds), string.Join(",", characterNames));
        }

        [RemoteEvent("CharacterSelected")]
        public async void OnCharacterSelected(Client client, uint characterId)
        {
            NAPI.Chat.SendChatMessageToPlayer(client, "Selected Character ID " + characterId);

            var characterData = await CharacterData.GetCharacterData(client, characterId);
            client.SetData(CharacterData.CHARACTER_DATA_ID, characterData);

            if (characterData.SpawnPosition == null)
            {
                var spawnPoint = spawnPositions.Locations[Main.Random.Next(spawnPositions.Locations.Length)];
                var pos = new Vector3(spawnPoint.X, spawnPoint.Y, spawnPoint.Z);
                NAPI.Player.SpawnPlayer(client, pos, spawnPoint.Heading);
                NAPI.Chat.SendChatMessageToPlayer(client, "Spawned at random pos");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(client, "Spawned at old position " + characterData.SpawnPosition);
                NAPI.Player.SpawnPlayer(client, characterData.SpawnPosition, 0);
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client client, DisconnectionType type, string reason)
        {
            var charData = client.GetData(CharacterData.CHARACTER_DATA_ID);
            if (charData == null) return;
            charData.SpawnPosition = client.Position;
            CharacterData.Save(charData);
        }



        [Command("savepos")]
        public void OnSavePos(Client client)
        {
            var charData = client.GetData(CharacterData.CHARACTER_DATA_ID);
            charData.SpawnPosition = client.Position;
            CharacterData.Save(charData);
        }


    }
}
