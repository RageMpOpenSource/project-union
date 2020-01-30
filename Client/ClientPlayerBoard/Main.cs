using RAGE;
using RAGETimer.Shared;
using System;
using System.Collections.Generic;

namespace ClientPlayerBoard
{
    public class ClientPlayerBoard : Events.Script
    {
        private RAGE.Ui.HtmlWindow playerBoard = null;
        private bool _isAllowedToRefreshPlayerboard = true;
        private uint _serverUpdateCooldown = 2000;
        public ClientPlayerBoard()
        {
            Events.Add("PlayerBoardUpdate", OnPlayerBoardUpdate);

            playerBoard = new RAGE.Ui.HtmlWindow("package://cs_packages/ClientPlayerBoard/PlayerBoard.html");
            playerBoard.Active = false;

            Events.Tick += OnTick;

            new Timer(() =>
            {
                _isAllowedToRefreshPlayerboard = true;
            }, _serverUpdateCooldown, 0);
        }

        private void OnPlayerBoardUpdate(object[] args)
        {
            string indexes = args[0].ToString();
            string names = args[1].ToString();
            playerBoard.ExecuteJs($"UpdatePlayerList('{indexes}', '{names}')");
        }

        private void OnTick(List<Events.TickNametagData> nametags)
        {
            playerBoard.Active = Input.IsDown((int)ConsoleKey.F3);

            if (playerBoard.Active && _isAllowedToRefreshPlayerboard)
            {
                Chat.Output("Requested Refresh");
                RAGE.Events.CallRemote("RequestPlayerBoardUpdate");
                _isAllowedToRefreshPlayerboard = false;
            }

        }
    }
}
