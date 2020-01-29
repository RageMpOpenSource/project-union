using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectUnion
{
    public class Logger
    {
        public void Log(string message)
        {
            NAPI.Util.ConsoleOutput($"[SERVER]: {message}");
        }
        public void LogWarning(string message)
        {
            NAPI.Util.ConsoleOutput(message, ConsoleColor.Yellow);
        }
        public void LogError(string message)
        {
            NAPI.Util.ConsoleOutput($"[SERVER ERROR]: {message}", ConsoleColor.Red);
        }

        public void LogClient(Client client, string message)
        {
            NAPI.Chat.SendChatMessageToPlayer(client, $"[SERVER]: {message}");
        }
        public void LogClientWarning(Client client, string message)
        {
            NAPI.Chat.SendChatMessageToPlayer(client, message);
        }
        public void LogClientError(Client client, string message)
        {
            NAPI.Chat.SendChatMessageToPlayer(client, $"[SERVER ERROR]: {message}");
        }

        public void LogAllClients(string message)
        {
            NAPI.Chat.SendChatMessageToAll($"[SERVER]: {message}");
        }
        public void LogAllClientsWarning(string message)
        {
            NAPI.Chat.SendChatMessageToAll(message);
        }
        public void LogAllClientsError(string message)
        {
            NAPI.Chat.SendChatMessageToAll($"[SERVER ERROR]: {message}");
        }

    }
}
