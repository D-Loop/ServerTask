using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerTask.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace ServerTask.LocalPlugins.BasicFunctions.Commands
{
    public class GetAllApplications: ICommandServer
    {

        //комманда добавления пользователя в группу
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                var listApp = new List<Application>();
                // Запрос для получения информации о процессах, включая использование CPU, память, диск и сеть
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.MainWindowTitle) && !process.MainWindowTitle.EndsWith(".exe"))
                        {
                            var app = new Application
                            {
                                Name = process.MainWindowTitle,
                                Id = process.Id,
                                CPUUsed = process.TotalProcessorTime.TotalMilliseconds / (Environment.ProcessorCount * 10),
                                MemoryUsed = (process.WorkingSet64 + process.PrivateMemorySize64) / (1024.0 * 1024.0)
                            };

                            listApp.Add(app);
                        }
                    }
                    catch (Exception ex)
                    {
                        //
                    }
                }

                return JsonConvert.SerializeObject(listApp.OrderBy(p => p.Name));

            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e);
            }
        }

    }
}
