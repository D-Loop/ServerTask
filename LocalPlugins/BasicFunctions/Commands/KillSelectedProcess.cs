using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerTask.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServerTask.LocalPlugins.BasicFunctions.Commands
{
    public class KillSelectedProcess : ICommandServer
    {

        //комманда добавления пользователя в группу
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                var currentProcessId = JsonConvert.DeserializeObject<int>(elMessageServer.Data);

                Process process = Process.GetProcessById(currentProcessId);

                if (process != null)
                {
                    // Закрываем процесс
                    process.Kill();
                    return JsonConvert.SerializeObject(new {Result = true});
                }

                return JsonConvert.SerializeObject(new {Result = false});
            }
            catch (Exception )
            {
                return JsonConvert.SerializeObject(new {Result = false});
            }
        }

    }
}
