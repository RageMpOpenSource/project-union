using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectUnion.Player.Commands
{
    interface ICommands
    {
        List<string> GetAllCommands();
        void GetCommandHelpResponse(out string title, out List<string> commands);
    }
}
