using GTANetworkAPI;
using System;

namespace ProjectUnionFreeroam.Utilities
{
    public class Logger
    {
        public void Log(string message)
        {
            NAPI.Util.ConsoleOutput($"[Server Log] {message}", ConsoleColor.Green);
        }
        public void LogWarning(string message)
        {
            NAPI.Util.ConsoleOutput($"[Server Log WARNING] {message}", ConsoleColor.Yellow);
        }
        public void LogError(string message)
        {
            NAPI.Util.ConsoleOutput($"[Server Log ERROR] {message}", ConsoleColor.Red);
        }
    }
}
