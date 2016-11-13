using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    public class NotificationsController : ApiController
    {
        [HttpGet()]
        public async Task<string> Send()
        {
            var message = "Amber's Army Needs You!";
            var connectionString = "Endpoint=sb://ambersarmy.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=m6wgOgKu8SYN+xLRzUSl8ZsE+dBaeaOONChKhfYE7eU=";
            var name = "ambersarmy";
            var hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, name);

            var windowsToastPayload = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" + message + @"</text></binding></visual></toast>";

            try
            {
                // Send the push notification.
                var result = await hub.SendWindowsNativeNotificationAsync(windowsToastPayload);
                return result.Success.ToString();

            }
            catch (System.Exception ex)
            {
                return "err";
            }
        }
    }
}
