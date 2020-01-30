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

        protected override void OnAboutToStart()
        {
            Data.WeaponChosen = GetMapData().WeaponList[Main.Random.Next(GetMapData().WeaponList.Length)];
        }

        protected override void OnAddPlayer(Client client)
        {
        }

        protected override void OnStart()
        {
            Data.TotalKills = new int[Data.CurrentPlayers.Count];
            TeleportPlayersIn();
            StashPlayerWeapons();
            GiveWeaponToPlayers();
            LogAllCurrentPlayers($"Kill everyone on sight! First one to {Data.KillTarget} wins!");
        }

        private void StashPlayerWeapons()
        {
            for (int i = 0; i < GetGameModeData().CurrentPlayers.Count; i++)
            {
                Client client = GetGameModeData().CurrentPlayers[i];
                Data.WeaponStash.Add(i, new List<StoredWeaponData>());

                if (client.Weapons == null)
                {
                    Data.WeaponStash[i] = new List<StoredWeaponData>();
                    continue;
                }

                foreach (WeaponHash weapon in client.Weapons)
                {
                    var storedData = new StoredWeaponData()
                    {
                        Hash = weapon,
                        Ammo = NAPI.Player.GetPlayerWeaponAmmo(client, weapon)
                    };
                    Data.WeaponStash[i].Add(storedData);
                }
            }
        }

        private void RestorePlayerWeapons()
        {
            for (int i = 0; i < GetGameModeData().CurrentPlayers.Count; i++)
            {
                Client client = GetGameModeData().CurrentPlayers[i];
                client.RemoveAllWeapons();

                List<StoredWeaponData> storedWeapons = Data.WeaponStash[i];

                foreach (StoredWeaponData storedWeaponData in storedWeapons)
                {
                    NAPI.Player.GivePlayerWeapon(client, storedWeaponData.Hash, storedWeaponData.Ammo);
                }
            }
        }

        private void GiveWeaponToPlayers()
        {
            foreach (Client client in Data.CurrentPlayers)
            {
                GiveWeaponToPlayer(client);
            }
        }

        private void GiveWeaponToPlayer(Client client)
        {
            NAPI.Player.GivePlayerWeapon(client, Data.WeaponChosen, 200);
        }

        protected override void OnStartCountdown()
        {
        }

        protected override void OnStop()
        {
            RestorePlayerWeapons();
            TeleportPlayersOut();
        }

        protected override void OnTick()
        {
            for (int i = 0; i < Data.TotalKills.Length; i++)
            {
                int kills = Data.TotalKills[i];
                if (kills >= Data.KillTarget)
                {
                    LogAllCurrentPlayers($"{Data.CurrentPlayers[i].Name} wins!");
                    StopGameMode();
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
    }
}
