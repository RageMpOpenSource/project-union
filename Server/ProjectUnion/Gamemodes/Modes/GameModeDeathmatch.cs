using GTANetworkAPI;
using System;

namespace ProjectUnion.GameModes.Modes
{

    public class GameModeDataDeathmatch : BaseGameModeData
    {

    }

    public class GameModeDeathmatch : BaseGameMode
    {
        public GameModeDeathmatch(uint id, Client host) : base(new GameModeDataDeathmatch(), id, host, "Deathmatch")
        {
        }

        public override void OnAddPlayer(Client client)
        {
        }
    }
}
