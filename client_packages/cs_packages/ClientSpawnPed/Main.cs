using RAGE;

namespace ClientSpawnPed
{
    public class Main : Events.Script
    {
        public Main()
        {
            Events.Add("FreezePlayer", OnFreezePlayer);
        }

        private void OnFreezePlayer(object[] args)
        {
            var isFrozen = (bool)args[0];
            var freezeSourceName = (string)args[1];
            var frozenString = isFrozen ? "frozen" : "unfrozen";
            var outputText = $"You were {frozenString} by {freezeSourceName}";
            RAGE.Chat.Output(outputText);
            RAGE.Elements.Player.LocalPlayer.FreezePosition(isFrozen);
        }
    }
}
