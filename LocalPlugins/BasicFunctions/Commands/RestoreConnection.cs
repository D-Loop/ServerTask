using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;

namespace ServerTask.LocalPlugins.BasicFunctions.Commands
{
   //Команда на восстоновление соединений по листу сервера
    class RestoreConnection : ICommandServer
    {
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                //Проверяем наличие соединения с данным ключом в списке активных соединений сервера 
                if (elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections.ContainsKey(
                    $"{elMessageServer.GUID}"))
                {
                    //Переносим настройки в текущее подключение
                    elConnectionClient.SessionUser = elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections[$"{elMessageServer.GUID}"].SessionUser;
                    elConnectionClient.ConnectionSettings = elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections[$"{elMessageServer.GUID}"].ConnectionSettings;
                    //заменяем в словаре активных подключений по ключу соединения на текущее
                    elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections[
                        $"{elMessageServer.GUID}"] = elConnectionClient;
                }
                    
                //возвращаем настройки подключения, при отсутствии данных пользователя система не восстановит авторизацию
                return JsonConvert.SerializeObject(elConnectionClient.SessionUser);
            }
            catch (Exception e)
            {
                elConnectionClient.SessionUser = null;
                return null;
            }
        }

    }

}
