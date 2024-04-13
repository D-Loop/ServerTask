using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ElMessage;

namespace ServerTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try { ConfigServer.ServerIp = IPAddress.Parse("127.0.0.1"); }
            catch { ConfigServer.ServerIp = IPAddress.Any; }


            ConfigServer.ServerPort = ushort.Parse("61111");
            ConfigServer.ServerPluginsControl = new PluginsControl(ConfigServer.PluginsFolderPath);
            ConfigServer.ServerControlManager = new ElServerControlManager("NotDataBase", 1);
            //Назначение контралера выполнения комманд комманд
            ConfigServer.ServerControlManager.PluginControl += ConfigServer.ServerPluginsControl.SendCommandToPlugin;
            //Создаем и запускаем сервер*****************************


            ConfigServer.ServerControlManager.ListeningStart(ConfigServer.ServerIp, ConfigServer.ServerPort);

        }
    }
}
