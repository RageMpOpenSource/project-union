using GTANetworkAPI;
using ProjectUnion.Data;
using ProjectUnion.GameModes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectUnion.Server
{
    public class Commands : Script
    {

        public static List<string> GeneralCommands;
        public static List<string> AdminCommands;
        public static List<string> OwnerCommands;

        public Commands()
        {
            GeneralCommands = new List<string>() { COMMAND_VEHICLE_PARK };

            AdminCommands = new List<string>();
            AdminCommands.AddRange(GeneralCommands);
            AdminCommands.AddRange(new List<string>() { COMMAND_VEHICLE_CREATE });


            OwnerCommands = new List<string>();
            OwnerCommands.AddRange(AdminCommands);
            OwnerCommands.AddRange(new List<string>() { COMMAND_VEHICLE_RESPAWN_ALL, COMMAND_VEHICLE_SET_OWNER });
        }

        #region General Commands

        [Command("pos")]
        public void CMD_GetPosition(Client client)
        {
            Main.Logger.LogClient(client, $"Position: [{client.Position}]");
        }
        [Command("dc")]
        public void Disconnect(Client client)
        {
            NAPI.Player.KickPlayer(client);
        }

        #endregion

        #region Vehicle Commands

        public const string COMMAND_VEHICLE_CREATE = "createveh";
        public const string COMMAND_VEHICLE_RESPAWN_ALL = "respawnvehs";
        public const string COMMAND_VEHICLE_PARK = "parkveh";
        public const string COMMAND_VEHICLE_SET_OWNER = "setvehowner";


        /// <summary>
        /// Spawn a vehicle
        /// </summary>
        /// <param name="client"></param>
        /// <param name="vehName"></param>
        [Command(COMMAND_VEHICLE_CREATE)]
        public async void SpawnVeh(Client client, string vehName)
        {
            if (await ServerUtilities.CanUseCommand(client, COMMAND_VEHICLE_CREATE) == false) return;
            uint vehHash = NAPI.Util.GetHashKey(vehName);
            var vehicle = NAPI.Vehicle.CreateVehicle(vehHash, client.Position.Around(5), client.Heading, 112, 112);
            Main.Logger.LogClient(client, $"Spawned vehicle ({vehicle.DisplayName}).");
        }

        /// <summary>
        /// Respawn all vehicles (removing vehicles without owners)
        /// </summary>
        /// <param name="client"></param>
        [Command(COMMAND_VEHICLE_RESPAWN_ALL)]
        public async void CMD_ForceRespawnVehicles(Client client)
        {
            if (await ServerUtilities.CanUseCommand(client, COMMAND_VEHICLE_RESPAWN_ALL) == false) return;
            VehicleRespawner.RespawnVehicles();
        }

        [Command(COMMAND_VEHICLE_PARK)]
        public async void CMD_ParkVehicle(Client client)
        {
            if (await ServerUtilities.CanUseCommand(client, COMMAND_VEHICLE_PARK) == false) return;
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(client);
            if (vehicle == null) return;

            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (characterData == null) return;

            VehicleData vehicleData = vehicle.GetData(VehicleData.VEHICLE_DATA_KEY);

            //No owner
            if (vehicleData == null)
            {
                client.SendChatMessage("You do not own this vehicle!");
                return;
            }

            if (vehicleData.OwnerId != characterData.Id)
            {
                client.SendChatMessage("You do not own this vehicle!");
                return;
            }

            vehicleData.SetPosition(vehicle.Position);
            vehicleData.Heading = vehicle.Heading;
            VehicleDatabase.SaveVehicle(vehicleData);
            Main.Logger.LogClient(client, $"({vehicle.DisplayName}) parked at {vehicle.Position}");
        }


        [Command(COMMAND_VEHICLE_SET_OWNER)]
        public async void CMD_TakeOwnershipOfVehicle(Client client, string targetFirstName, string targetSurname = "")
        {
            if (await ServerUtilities.CanUseCommand(client, COMMAND_VEHICLE_SET_OWNER) == false) return;

            Client owner = ServerUtilities.GetPlayerIfExists(client, targetFirstName, targetSurname);
            if (owner == null) return;

            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(owner);
            if (vehicle == null)
            {
                Main.Logger.LogClient(client, "You are not in a vehicle!");
                return;
            }

            CharacterData newOwnerCharacterData = owner.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (newOwnerCharacterData == null) return;

            CharacterData clientCharacterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);
            //Owner is Client
            bool isClientOwner = false;
            if (clientCharacterData != null && clientCharacterData.Id == newOwnerCharacterData.Id)
            {
                isClientOwner = true;
            }

            VehicleData vehicleData = vehicle.GetData(VehicleData.VEHICLE_DATA_KEY);

            if (vehicleData != null)
            {
                if (vehicleData.OwnerId == newOwnerCharacterData.Id)
                {
                    if (isClientOwner)
                    {
                        Main.Logger.LogClient(client, $"You already own ({vehicleData.Id}) {vehicle.DisplayName}.");
                    }
                    else
                    {
                        Main.Logger.LogClient(client, $"{newOwnerCharacterData.Name} already owns ({vehicleData.Id}) {vehicle.DisplayName}.");
                    }
                    return;
                }
                else
                {
                    vehicleData.OwnerId = newOwnerCharacterData.Id;
                    VehicleDatabase.SaveVehicle(vehicleData);
                }
            }
            else
            {
                vehicleData = new VehicleData()
                {
                    VehicleName = ((VehicleHash)vehicle.Model).ToString(),
                    Color1 = vehicle.PrimaryColor,
                    Color2 = vehicle.SecondaryColor,
                    Heading = vehicle.Heading,
                    OwnerId = newOwnerCharacterData.Id
                };

                vehicleData.SetPosition(vehicle.Position);

                vehicleData = await VehicleDatabase.CreateVehicle(vehicleData);
                vehicle.SetData(VehicleData.VEHICLE_DATA_KEY, vehicleData);
            }


            Main.Logger.LogClient(client, $"Gave {newOwnerCharacterData.Name} ownership of {vehicle.DisplayName}.");
            Main.Logger.LogClient(owner, $"{clientCharacterData.Name} gave you ownership of {vehicle.DisplayName}.");
        }


        #endregion


        #region Ped Commands
        [Command("friend")]
        public void CMD_CreatePed(Client client)
        {
            uint tempModel = NAPI.Util.GetHashKey("ig_ballasog");
            NAPI.ClientEvent.TriggerClientEvent(client, "PedCreated", tempModel, client.Position, client.Heading);
        }
        #endregion


        #region GameMode Commands

        public const string COMMAND_GAMEMODE_CREATE = "creategm";
        public const string COMMAND_GAMEMODE_GET_MY_GAMEMODES = "mygms";
        public const string COMMAND_GAMEMODE_SET_MAP = "setgmmap";


        [Command(COMMAND_GAMEMODE_CREATE)]
        public void CreateGameMode(Client client, int gmType)
        {
            try
            {
                uint gameModeId = Main.GameModeHandler.CreateGameMode(client, (GameModeType)gmType);
                Main.Logger.LogClient(client, $"Created Game Mode with Id {gameModeId}");
            }
            catch (Exception e)
            {
                Main.Logger.LogClient(client, e.Message);
            }
        }

        [Command(COMMAND_GAMEMODE_GET_MY_GAMEMODES)]
        public void GetMyGameModes(Client client)
        {
            try
            {
                List<BaseGameMode> myGameModes = Main.GameModeHandler.GetGameModeByHost(client);
                string response = "";

                if (myGameModes.Count == 0)
                {
                    response += "You are not hosting any game modes.";
                }

                for (int i = 0; i < myGameModes.Count; i++)
                {
                    BaseGameMode gm = myGameModes[i];
                    response += $"({gm.GetGameModeData().Id}) {gm.GetGameModeData().Name}";

                    if (i < myGameModes.Count - 1)
                    {
                        response += ",";
                    }
                }
                Main.Logger.LogClient(client, response);
            }
            catch (Exception e)
            {
                Main.Logger.LogClient(client, e.Message);
            }
        }

        [Command(COMMAND_GAMEMODE_SET_MAP)]
        public void SetGameModeMap(Client client, uint gmId, uint mapId)
        {
            try
            {
                BaseGameMode gameMode = Main.GameModeHandler.GetGameModeById(gmId);
                gameMode.SetGameModeMapId(mapId);
            }
            catch (Exception e)
            {
                Main.Logger.LogClient(client, e.Message);
            }
        }

        [Command("addplayertogm")]
        public void AddPlayerToGameMode(Client client, uint gmId, string playerFirstName, string playerSecondName = "")
        {
            Client player = ServerUtilities.GetPlayerIfExists(client, playerFirstName, playerSecondName);
            if (player == null)
            {
                return;
            }

            try
            {
                BaseGameMode gameMode = Main.GameModeHandler.GetGameModeById(gmId);
                gameMode.AddPlayer(player);
                Main.Logger.LogClient(gameMode.GetGameModeData().EventHost, $"{client.Name} joined the event ({gameMode.GetGameModeData().Id}) {gameMode.GetGameModeData().Name}.");
                Main.Logger.LogClient(client, $"You joined the event ({gameMode.GetGameModeData().Id}) {gameMode.GetGameModeData().Name}.");
            }
            catch (Exception e)
            {
                Main.Logger.LogClient(client, e.Message);
            }
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnChatTest(Client client, uint gmId, string message)
        {
            if (message.Trim() == COMMAND_GAMEMODE_SET_MAP)
            {
                try
                {
                    BaseGameMode gameMode = Main.GameModeHandler.GetGameModeById(gmId);
                    List<BaseGameModeMapData> suitableMaps = Main.GameModeHandler.GetSuitableMaps(gameMode.GetGameModeData().Type);

                    string response = "";
                    for (int i = 0; i < suitableMaps.Count; i++)
                    {
                        BaseGameModeMapData mapData = suitableMaps[i];
                        response += $"({mapData.MapId}) ${mapData.DisplayName}";

                        if (i < suitableMaps.Count - 1)
                        {
                            response += ",";
                        }
                    }

                    Main.Logger.LogClient(client, "Suitable Maps:");
                    Main.Logger.LogClient(client, response);
                }
                catch (Exception e)
                {
                    Main.Logger.LogClient(client, e.Message);
                }
            }
        }

        #endregion

    }
}
