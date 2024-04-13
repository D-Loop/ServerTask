using ElMessage;
using ElMessage.Interface;
using Newtonsoft.Json;
using NLog;
using System;

namespace ServerTask.LocalPlugins.BasicFunctions.Commands
{
    public class Authorization:ICommandServer
    {
        //комманда добавления пользователя в группу
        public string Execute(ElMessageServer elMessageServer, ElConnectionClient elConnectionClient)
        {
            try
            {
                var currentUser = JsonConvert.DeserializeObject<ElUserData>(elMessageServer.Data);
                currentUser.UserID = 1;
                elConnectionClient.SessionUser = currentUser;
                return JsonConvert.SerializeObject(currentUser);
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                LogManager.GetCurrentClassLogger().Trace(elMessageServer.Data.Replace("\r\n", ""));
                return JsonConvert.SerializeObject(e);
            }
        }

    }
}
