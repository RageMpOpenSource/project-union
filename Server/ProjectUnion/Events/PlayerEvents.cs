using GTANetworkAPI;
using ProjectUnion.Data;
using ProjectUnion.GameModes;
using ProjectUnion.Server;
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



        [ServerEvent(Event.PlayerConnected)]
        public async void OnPlayerConnected(Client client)
        {
            PlayerData playerData = null;

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
            }


            client.SetData(PlayerData.PLAYER_DATA_KEY, playerData);

            PlayerTempData playerTempData = new PlayerTempData()
            {
                LoginIndex = ServerUtilities.GetPlayerLoginIndex()
            };
            client.SetData(PlayerTempData.PLAYER_TEMP_DATA_KEY, playerTempData);


            ServerUtilities.SetPlayerNametag(client);

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

            Vector3 position = characterData.GetPosition();
            float heading = characterData.Heading.HasValue ? characterData.Heading.Value : 0;

            if (position == null)
            {
                GamePosition spawnPoint = ServerUtilities.GetRandomSpawnPoint();
                position = spawnPoint.GetPosition();
                heading = spawnPoint.GetHeading();
            }


            client.SetData(CharacterData.CHARACTER_DATA_KEY, characterData);

            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_KEY);
            GroupData highestRankedGroup = await GroupDatabase.GetPlayerHighestRankingGroup(playerData.Id);

            if (highestRankedGroup == null) return;
            if (characterData == null) return;

            ServerUtilities.SetPlayerNametag(client);
            ServerUtilities.SwitchPlayerPosition(client, position, heading);
        }




        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            PlayerTempData playerTempData = client.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);

            if (playerTempData.GamemodeId.HasValue)
            {
                GameModeHandler.Instance.OnDeath(client, killer, reason);
                return;
            }

            ServerUtilities.SpawnPlayerAfter(client);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Client player, DisconnectionType type, string reason)
        {
            PlayerDatabase.SavePlayerData(player);
            CharacterDatabase.SaveCharacterData(player);

            player.ResetData(PlayerData.PLAYER_DATA_KEY);
            player.ResetData(CharacterData.CHARACTER_DATA_KEY);
            player.ResetData(PlayerTempData.PLAYER_TEMP_DATA_KEY);
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
