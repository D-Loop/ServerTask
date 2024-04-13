using ElMessage.Helper;
using ElMessage.Interface;
using ElMessage;
using System.Collections.Concurrent;
using System.Net;
using ServerTask.LocalPlugins.BasicFunctions.Commands;

namespace ServerTask.LocalPlugins.BasicFunctions
{
    public class BasicFunctions : IPluginServer
    {
        public BasicFunctions(ElServerControlManager serverControlManager)
        {
            ServerControlManager = serverControlManager;
            AllCommands = new ConcurrentDictionary<string, ICommandServer>();

            //Поднимаем обекты команд плагина
            AllCommands.TryAdd("Authorization", new Commands.Authorization());
            AllCommands.TryAdd("GetAllProcessesWithData", new Commands.GetAllProcessesWithData()); 
            AllCommands.TryAdd("KillSelectedProcess", new Commands.KillSelectedProcess());
            AllCommands.TryAdd("GetAllApplications", new Commands.GetAllApplications());
            AllCommands.TryAdd("RestoreConnection", new Commands.RestoreConnection());

        }

        public ElServerControlManager ServerControlManager { get; }

        public string Name { get; set; }
        public string Dll { get; set; }
        public long ServerVersion { get; set; }
        public long OldServerVersion { get; set; }

        public ConcurrentDictionary<string, ICommandServer> AllCommands { get; set; }

        public string GetCommand(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
            => AllCommands[elMessageServer.Command].Execute(elMessageServer, elConnectionClient);

        public bool ContainsCommand(string commandName) => AllCommands.ContainsKey(commandName);

    }
}

