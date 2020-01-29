using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using ProjectUnion.Data;

namespace ProjectUnion.Server
{
    public static class ServerUtilities
    {

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
    }
}
