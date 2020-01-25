using GTANetworkAPI;
using ProjectUnion.Player.Data;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
