using GTANetworkAPI;
using MySql.Data.MySqlClient;
using ProjectUnion.Data;
using ProjectUnion.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectUnion
{
    public class Main : Script
    {
        public static Logger Logger = new Logger();
        public static Random Random = new Random();
        public static MySqlConnection Connection;


        [ServerEvent(Event.ResourceStart)]
        public async void OnResourceStart()
        {

            //NAPI.Server.SetAutoRespawnAfterDeath(false);
            //NAPI.Server.SetAutoSpawnOnConnect(false);

            Database db = new Database();
            Connection = db.Connection;

            GroupDatabase.InitializeTable();
            GroupDatabase.InitializeGroups();
            VehicleDatabase.InitializeTable();
            PlayerDatabase.InitializeTable();
            CharacterDatabase.InitializeTable();


            await GroupDatabase.AddCommandsToGroup(Config.GROUP_NAME_ADMIN, Commands.AdminCommands.ToArray());
            await GroupDatabase.AddCommandsToGroup(Config.GROUP_NAME_OWNER, Commands.OwnerCommands.ToArray());
        }


    }
}
