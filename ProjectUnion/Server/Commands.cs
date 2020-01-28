using GTANetworkAPI;
using ProjectUnion.Data;
using System.Collections.Generic;
using System.Globalization;
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


        #region Utilities
        public async Task<bool> CanUseCommand(Client client, string command)
        {
            PlayerData playerData = client.GetData(PlayerData.PLAYER_DATA_KEY);
            if (playerData == null)
            {
                client.SendChatMessage("You are not logged in! Please reconnect.");
                return false;
            }

            bool canUse = await GroupDatabase.DoesPlayerHaveCommand(playerData.Id, command);
            if (canUse == false)
            {
                Main.Logger.LogClient(client, "You do not have access to this command.");
                return false;
            }

            return true;
        }

        public Client GetPlayerIfExists(Client client, string playerName)
        {
            playerName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerName);
            Client player = NAPI.Player.GetPlayerFromName(playerName);
            if (player == null)
            {

                Main.Logger.LogClient(client, $"Player {playerName} not found.");
                return null;
            }

            return player;
        }

        #endregion

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
            if (await CanUseCommand(client, COMMAND_VEHICLE_CREATE) == false) return;
            uint vehHash = NAPI.Util.GetHashKey(vehName);
            NAPI.Vehicle.CreateVehicle(vehHash, client.Position.Around(5), client.Heading, 112, 112);
        }

        /// <summary>
        /// Respawn all vehicles (removing vehicles without owners)
        /// </summary>
        /// <param name="client"></param>
        [Command(COMMAND_VEHICLE_RESPAWN_ALL)]
        public async void CMD_ForceRespawnVehicles(Client client)
        {
            if (await CanUseCommand(client, COMMAND_VEHICLE_RESPAWN_ALL) == false) return;
            VehicleRespawner.RespawnVehicles();
        }

        [Command(COMMAND_VEHICLE_PARK)]
        public async void CMD_ParkVehicle(Client client)
        {
            if (await CanUseCommand(client, COMMAND_VEHICLE_PARK) == false) return;
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(client);
            if (vehicle == null) return;

            CharacterData characterData = client.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (characterData == null) return;

            VehicleData vehicleData = vehicle.GetData(VehicleData.VEHICLE_DATA_KEY);
            if (vehicleData != null)
            {
                if (vehicleData.OwnerId != characterData.Id)
                {
                    client.SendChatMessage("You do not own this vehicle!");
                    return;
                }

                vehicleData.SetPosition(vehicle.Position);
                vehicleData.Heading = vehicle.Heading;
                VehicleDatabase.SaveVehicle(vehicleData);
            }
            else
            {
                client.SendChatMessage("You do not own this vehicle!");
            }
        }


        [Command(COMMAND_VEHICLE_SET_OWNER)]
        public async void CMD_TakeOwnershipOfVehicle(Client client, string ownerName)
        {
            if (await CanUseCommand(client, COMMAND_VEHICLE_SET_OWNER) == false) return;

            Client owner = GetPlayerIfExists(client, ownerName);
            if (owner == null) return;

            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(owner);
            if (vehicle == null)
            {
                Main.Logger.LogClient(client, "You are not in a vehicle!");
                return;
            }

            CharacterData ownerData = owner.GetData(CharacterData.CHARACTER_DATA_KEY);

            if (ownerData == null) return;

            CharacterData clientData = client.GetData(CharacterData.CHARACTER_DATA_KEY);
            //Owner is Client
            bool isClientOwner = false;
            if (clientData != null && clientData.Id == ownerData.Id)
            {
                isClientOwner = true;
            }

            VehicleData vehicleData = vehicle.GetData(VehicleData.VEHICLE_DATA_KEY);
            if (vehicleData != null)
            {
                if (vehicleData.OwnerId == ownerData.Id)
                {
                    if (isClientOwner)
                    {
                        Main.Logger.LogClient(client, "You already own this vehicle!");
                    }
                    else
                    {
                        Main.Logger.LogClient(client, $"{ownerData.Name} already owns this vehicle!");
                    }
                    return;
                }
            }


            vehicleData = new VehicleData()
            {
                VehicleName = ((VehicleHash)vehicle.Model).ToString(),
                Color1 = vehicle.PrimaryColor,
                Color2 = vehicle.SecondaryColor,
                Heading = vehicle.Heading,
                OwnerId = ownerData.Id
            };

            vehicleData.SetPosition(vehicle.Position);

            vehicleData = await VehicleDatabase.CreateVehicle(vehicleData);
            vehicle.SetData(VehicleData.VEHICLE_DATA_KEY, vehicleData);


            Main.Logger.LogClient(client, $"Gave {owner.Name} ownership of {vehicle.DisplayName}.");
            Main.Logger.LogClient(owner, $"{client.Name} gave you ownership of {vehicle.DisplayName}.");
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

    }
}
