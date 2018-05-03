using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Server.Hubs
{
    [HubName("ZeroMessenger")]
    public class ChatHub : Hub
    {
        internal static Dictionary<string, long> users = new Dictionary<string, long>();

        public async Task connect(string accessToken)
        {
            var task = new Controllers.TokensController().ValidToken(accessToken);
            var user = await task;
            if (user != null)
            {
                foreach (var chatId in await (new Controllers.UsersController().GetAllChats(user.UserId)))
                {
                    await Groups.Add(Context.ConnectionId, chatId.ToString());
                }
                try
                {
                    users.Add(Context.ConnectionId, user.UserId);
                }
                catch { }
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (users.Keys.Contains(Context.ConnectionId))
            { 
                users.Remove(Context.ConnectionId);
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}