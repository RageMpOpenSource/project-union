using GTANetworkAPI;
using ProjectUnion.Data;
using System;
using System.Collections.Generic;

namespace ProjectUnion.Server
{
    public class ServerAutoSaving : Script
    {

        private System.Threading.Timer _autoSaveTimer;
        private const int AUTO_SAVE_TIME_IN_MS = 30000;
        public ServerAutoSaving()
        {

            _autoSaveTimer = new System.Threading.Timer(SavePlayer, null, AUTO_SAVE_TIME_IN_MS, System.Threading.Timeout.Infinite);

        }

        private void SavePlayer(object state)
        {
            _autoSaveTimer.Change(AUTO_SAVE_TIME_IN_MS, System.Threading.Timeout.Infinite);

            List<Client> clients = NAPI.Pools.GetAllPlayers();

            foreach (Client client in clients)
            {
                CharacterDatabase.SaveCharacterData(client);
            }
        }
    }
}
