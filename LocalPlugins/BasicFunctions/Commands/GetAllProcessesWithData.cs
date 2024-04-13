using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using ServerTask.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ServerTask.LocalPlugins.BasicFunctions.Commands
{
    public class GetAllProcessesWithData : ICommandServer
    {

        //комманда добавления пользователя в группу
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                var listProcesses = new List<ProcessData>();
                var tmpProcess = new ProcessData();

                foreach (Process process in processes)
                {
                    try
                    {
                        tmpProcess = new ProcessData();    
                        // Пытаемся получить информацию о процессе
                        tmpProcess.Name = process.ProcessName;
                        tmpProcess.Id = process.Id;
                        tmpProcess.MemoryUsed = process.WorkingSet64 / 1024; // Память используемая процессом в KБ
                        try
                        {
                            tmpProcess.Description = process.MainModule.FileVersionInfo.FileDescription;  // Описание процесса
                        }
                        catch (Exception)
                        {
                            //
                        }

                        if (process.StartTime != default(DateTime)) // Проверяем, что время запуска процесса не равно значению по умолчанию
                            tmpProcess.ProcessTime = DateTime.Now - process.StartTime; // Время работы процесса
                        else
                            tmpProcess.ProcessTime = TimeSpan.Zero; // Устанавливаем время работы процесса в ноль, если время запуска процесса неизвестно

                        // Добавляем информацию о процессе в список
                        listProcesses.Add(tmpProcess);
                    }
                    catch (Exception )
                    {
                        continue;
                    }
                }

                return JsonConvert.SerializeObject(listProcesses.OrderBy(p=>p.Name));
            }
            catch (Exception e)
            {
                // Обработка других исключений
                return JsonConvert.SerializeObject(e);
            }
        }

    }
}
