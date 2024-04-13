using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using ElMessage;
using ElMessage.Helper;
using ElMessage.Interface;
using ElMessage.Enum;
using ServerTask.LocalPlugins.BasicFunctions;
using NLog;
using Newtonsoft.Json;

namespace ServerTask
{
    public class PluginsControl
    {
        
        public ConcurrentDictionary<string, IPluginServer> AllPlugins;
        public ConcurrentDictionary<string, IElMicroservice> AllMicroservices;
        private readonly string _pluginsServerPath;
        private  ElServerControlManager _serverControl;
        private  Dictionary<string, long> ListPlugin { get; set; }

        public PluginsControl(string pluginsServerPath)
        {
            _pluginsServerPath = pluginsServerPath;
            AllPlugins = new ConcurrentDictionary<string, IPluginServer>();
            AllMicroservices = new ConcurrentDictionary<string, IElMicroservice>();
            LoadPlugin();

        }
        
        public void SetServerControlManager(ElServerControlManager serverControl)
        {
            _serverControl = serverControl;
            _serverControl.ListPlugin = ListPlugin;
        }
        
        public void LoadPlugin()
        {
            IPluginServer newPlugin;

            newPlugin = new BasicFunctions(ConfigServer.ServerControlManager);
            AllPlugins.TryAdd("BasicFunctions", newPlugin);
        }

        public string SendCommandToPlugin(ElMessageServer elMessageServer, ElConnectionClient currentElConnectionClient)
        {
            try {
#if DEBUG
            Console.WriteLine("Request{3}:{0}/{1} => {2} \r\n", elMessageServer.PluginName, elMessageServer.Command, elMessageServer.Data, elMessageServer.GUID);
#endif
                
                    //регестрируем флаг внешнего обращения и сбрасываем в сообщении
                    var isExternal = elMessageServer.IsExternal?1:0;
                    elMessageServer.IsExternal = false;
                    //***************************************
                    if (!AllPlugins.ContainsKey(elMessageServer.PluginName))
		               throw new Exception($"Расширение {elMessageServer.PluginName} не поддерживается сервером");
		            if (!AllPlugins[elMessageServer.PluginName].ContainsCommand(elMessageServer.Command))
		               throw new Exception($"Команда  {elMessageServer.Command} расширения {elMessageServer.PluginName} не поддерживается расширением");
                   
                    var startTime = DateTime.Now;
                    var stopWatch = new Stopwatch();
               
                    stopWatch.Start();
		            var dataResponse = AllPlugins[elMessageServer.PluginName].GetCommand(elMessageServer, currentElConnectionClient);
                    stopWatch.Stop();
            
                    var millisecondInterval = stopWatch.Elapsed.TotalMilliseconds;
		          

#if DEBUG
		            Console.WriteLine("Response{3}:{0}/{1} => {2}\r\n", elMessageServer.PluginName, elMessageServer.Command, dataResponse, elMessageServer.GUID);
#endif
               
                if (currentElConnectionClient.SessionUser is null) return dataResponse;
                if (currentElConnectionClient.ConnectionSettings.IsSaveQueryLog)
                    AddLogQueryServer(currentElConnectionClient.ConnectionSettings,
                        $"{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{isExternal}");

                if (currentElConnectionClient.ConnectionSettings.IsSaveInputDataLog)
                    AddLogQueryDataServer(currentElConnectionClient.ConnectionSettings,
                        $"Request=>{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{elMessageServer.Data}");

                if (currentElConnectionClient.ConnectionSettings.IsSaveOutputDataLog)
                    AddLogQueryDataServer(currentElConnectionClient.ConnectionSettings,
                        $"Response=>{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{dataResponse}");

                return dataResponse;
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
                LogManager.GetCurrentClassLogger().Error($"Ошибка обращения к серверу: {e.ToString().Replace("\r\n", "")}");
                return JsonConvert.SerializeObject(e);
            }

        }



        public string SendCommandToPlugin(IElMessage elMessageServer, IConnection currentElConnectionClient)
        {
            try
            {
#if DEBUG
                Console.WriteLine("Request{3}:{0}/{1} => {2} \r\n", elMessageServer.PluginName, elMessageServer.Command, elMessageServer.Data, elMessageServer.GUID);
#endif

                //регестрируем флаг внешнего обращения и сбрасываем в сообщении
                var isExternal = elMessageServer.IsExternal ? 1 : 0;
                elMessageServer.IsExternal = false;
                //***************************************
                if (!AllMicroservices.ContainsKey(elMessageServer.PluginName))
                    throw new Exception($"Микросервис {elMessageServer.PluginName} не доступен на текущем хоте");
                if (!AllMicroservices[elMessageServer.PluginName].ContainsCommand(elMessageServer.Command))
                    throw new Exception($"Команда  {elMessageServer.Command} => {elMessageServer.PluginName} не поддерживается в версии микросервиса [{AllMicroservices[elMessageServer.PluginName].ServerVersion}]");

                var startTime = DateTime.Now;
                var stopWatch = new Stopwatch();

                stopWatch.Start();
                var dataResponse = AllMicroservices[elMessageServer.PluginName].GetCommand(elMessageServer, currentElConnectionClient);
                stopWatch.Stop();

                if (elMessageServer.StatusCode == EResponseStatusCode.Processing) elMessageServer.StatusCode = EResponseStatusCode.OK;

                var millisecondInterval = stopWatch.Elapsed.TotalMilliseconds;


#if DEBUG
                Console.WriteLine("Response{3}:{0}/{1} => {2}\r\n", elMessageServer.PluginName, elMessageServer.Command, dataResponse, elMessageServer.GUID);
#endif

                if (currentElConnectionClient.SessionUser is null) return dataResponse;
                if (currentElConnectionClient.ConnectionSettings.IsSaveQueryLog)
                    AddLogQueryServer(currentElConnectionClient.ConnectionSettings,
                        $"{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{isExternal}");

                if (currentElConnectionClient.ConnectionSettings.IsSaveInputDataLog)
                    AddLogQueryDataServer(currentElConnectionClient.ConnectionSettings,
                        $"Request=>{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{elMessageServer.Data}");

                if (currentElConnectionClient.ConnectionSettings.IsSaveOutputDataLog)
                    AddLogQueryDataServer(currentElConnectionClient.ConnectionSettings,
                        $"Response=>{startTime:O}|{millisecondInterval}|{currentElConnectionClient.SessionUser.UserID}|{elMessageServer.PluginName}|{elMessageServer.Command}|{dataResponse}");

                return dataResponse;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
                LogManager.GetCurrentClassLogger().Error($"Ошибка обращения к серверу: {e.ToString().Replace("\r\n", "")}");
                return JsonConvert.SerializeObject(e);
            }

        }







        private async void AddLogQueryServer(ConnectionSettings setting, string data)
        {
          
            var i = 0;
            while (true)
            {

                try
                {
                    using (var sw = new StreamWriter(setting.LogProtocolPath + $@"log_query_server/{DateTime.Now:dd-MM-yy}_{setting.ConnectionKey}.log", true, System.Text.Encoding.Default))
                    {
                        await sw.WriteLineAsync(data);
                    }
                    return;
                }
                catch (IOException e)
                {
                    if (i > 100)
                    {
                        LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                        return;
                    }

                    i++;
                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                    return;
                }
            }
        }

        private async void AddLogQueryDataServer(ConnectionSettings setting, string data)
        {
          
                var i = 0;
                while (true)
                {
                    
                    try
                    {
                        using (var sw = new StreamWriter(setting.LogProtocolPath + $@"log_query_data_server/{DateTime.Now:dd-MM-yy}_{setting.ConnectionKey}.log", true, System.Text.Encoding.Default))
                        {
                            await sw.WriteLineAsync(data);
                        }
                        return;
                    }
                    catch (IOException e)
                    {
                        if (i > 100)
                        {
                           LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                           return;
                        }

                        i++;
                        Thread.Sleep(150);
                    }
                    catch (Exception e)
                    {
                        LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                        return;
                    }
                }
        }

       



    }


  
}
