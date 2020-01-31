using GTANetworkAPI;
using ProjectUnion.Server;
using System;
using System.Collections.Generic;
using static ProjectUnion.GameModes.Modes.GameModeDataDeathmatch;

namespace ProjectUnion.GameModes.Modes
{

    public class GameModeDataDeathmatch : BaseGameModeData
    {
        public class StoredWeaponData
        {
            public WeaponHash Hash;
            public int Ammo;
        }

        public WeaponHash WeaponChosen;
        public int KillTarget = 5;


        public int[] TotalKills;
        public Dictionary<int, List<StoredWeaponData>> WeaponStash = new Dictionary<int, List<StoredWeaponData>>();

        public GameModeDataDeathmatch()
        {
            base.Type = GameModeType.Deathmatch;
        }
    }

    public class GameModeDeathmatch : BaseGameMode
    {
        private GameModeDataDeathmatch Data { get { return (GameModeDataDeathmatch)GameModeData; } }

        public GameModeDeathmatch(GameModeHandler gameModeHandler, uint id, Client host) : base(gameModeHandler, new GameModeDataDeathmatch(), id, host, "Deathmatch")
        {
            base.TickTime = 50;
        }


        protected override void OnAddPlayer(Client client)
        {
        }

        private void StashPlayerWeapons(Client client, int clientIndex)
        {
            Data.WeaponStash.Add(clientIndex, new List<StoredWeaponData>());

            if (client.Weapons == null)
            {
                return;
            }

            foreach (WeaponHash weapon in client.Weapons)
            {
                var storedData = new StoredWeaponData()
                {
                    Hash = weapon,
                    Ammo = NAPI.Player.GetPlayerWeaponAmmo(client, weapon)
                };
                Data.WeaponStash[clientIndex].Add(storedData);
            }
        }

        private void RestorePlayerWeapons(Client client, int clientIndex)
        {
            client.RemoveAllWeapons();

            List<StoredWeaponData> storedWeapons = Data.WeaponStash[clientIndex];

            foreach (StoredWeaponData storedWeaponData in storedWeapons)
            {
                NAPI.Player.GivePlayerWeapon(client, storedWeaponData.Hash, storedWeaponData.Ammo);
            }
        }


        private void GiveWeaponToPlayer(Client client)
        {
            NAPI.Player.GivePlayerWeapon(client, Data.WeaponChosen, 200);
        }

        protected override void OnPrepare()
        {
            Data.TotalKills = new int[Data.CurrentPlayers.Count];
            Data.WeaponChosen = GetMapData().WeaponList[Main.Random.Next(GetMapData().WeaponList.Length)];
        }


        protected override void OnSetPlayerState(Client client, int clientIndex)
        {
            StashPlayerWeapons(client, clientIndex);
            GiveWeaponToPlayer(client);
        }



        protected override void OnStart()
        {
            TeleportPlayersIn();
            LogAllCurrentPlayers($"Kill everyone on sight! First one to {Data.KillTarget} wins!");
        }
        protected override void OnResetPlayerState(Client client, int clientIndex)
        {
            RestorePlayerWeapons(client, clientIndex);
        }

        protected override void OnFinished()
        {
        }


        protected override void OnDestroy()
        {
        }

        protected override void OnTick()
        {
            if (IsGameModeFinished == false)
            {
                for (int i = 0; i < Data.TotalKills.Length; i++)
                {
                    int kills = Data.TotalKills[i];
                    if (kills >= Data.KillTarget)
                    {
                        LogAllCurrentPlayers($"{Data.CurrentPlayers[i].Name} wins!");
                        FinishGameMode();
                    }
                }
            }
        }

        protected override void OnPlayerDeath(Client client, Client killer, uint reason)
        {
            GamePosition randomSpawn = GetRandomSpawnPosition();

            var killerIndex = Data.CurrentPlayers.IndexOf(killer);
            if (killerIndex != -1)
                Data.TotalKills[killerIndex]++;

            ServerUtilities.SpawnPlayerAfter(client, randomSpawn, callback: () =>
            {
                GiveWeaponToPlayer(client);
            });
        }

        protected override void OnRemovePlayer(Client client)
        {
        }
    }
}
