using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ElMessage;
using ElMessage.Helper;

namespace ServerTask
{
    public static class ConfigServer
    {

        public static ElServerControlManager ServerControlManager { get; set; }
        public static PluginsControl ServerPluginsControl { get; set; }
        //public static ConfigurationServer Configuration { get; set; }
        public static IPAddress ServerIp { get; set; }
        public static ushort ServerPort { get; set; }
        public static string SqlServerConnectionString { get; set; }
        public static string Version { get; set; }

        /// <summary>Директория пакетов расширений </summary>
        public static string PluginsFolderPath { get; set; }

        /// <summary>Включить планировщик для запуска задание EL </summary>
        public static bool IsEnableTaskScheduler { get; set; }

        /// <summary>Включить планировщик для запуска команд EL </summary>
        public static bool IsEnableCommandScheduler { get; set; }


        //Конфигурация настроек точек соединений




    }
}
