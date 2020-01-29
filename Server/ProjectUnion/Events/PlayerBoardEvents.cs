using GTANetworkAPI;
using ProjectUnion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectUnion.Events
{
    public class PlayerBoardEvents : Script
    {

        [RemoteEvent("RequestPlayerBoardUpdate")]
        public void OnPlayerBoardUpdate(Client client)
        {
            List<Client> players = NAPI.Pools.GetAllPlayers();

            List<CharacterData> characterData = players.Select(e => (CharacterData)e.GetData(CharacterData.CHARACTER_DATA_KEY)).ToList();
            List<PlayerTempData> allPlayerTempData = players.Select(e => (PlayerTempData)e.GetData(PlayerTempData.PLAYER_TEMP_DATA_KEY)).ToList();

            List<uint> loginIndexes = allPlayerTempData.Select(e => e.LoginIndex).ToList();
            List<string> characterNames = characterData.Where(e => e != null).Select(e => e.Name).ToList();

            characterNames.AddRange(characterData.Where(e => e == null).Select(e => "Unknown Player"));

            NAPI.ClientEvent.TriggerClientEvent(client, "PlayerBoardUpdate", string.Join(",", loginIndexes), string.Join(",", characterNames));

        }
    }
}
