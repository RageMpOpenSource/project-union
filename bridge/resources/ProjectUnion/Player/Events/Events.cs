using GTANetworkAPI;
using ProjectUnion.Player.Data;

namespace ProjectUnion.Player.Events
{
    public class Events : Script
    {

        [ServerEvent(Event.PlayerConnected)]
        public async void OnPlayerConnected(Client client)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            if (playerData == null)
            {
                playerData = await PlayerData.CreatePlayerData(client);
                NAPI.Util.ConsoleOutput("New Player Created!");
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public async void OnPlayerDisconnected(Client client, DisconnectionType type, string reason)
        {
            var playerData = await PlayerData.GetPlayerData(client);
            var charData = await CharacterData.GetCharacterData(client, 1);
            charData.SpawnPosition = client.Position;
            CharacterData.Save(charData);
        }

    }
}
