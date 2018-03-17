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
        static Dictionary<string, int> users = new Dictionary<string, int>();
        public void connect(int userId, List<Models.Chats> chats)
        {
            foreach (var chat in chats)
            {
                Groups.Add(Context.ConnectionId, chat.Id.ToString());
            }
            users.Add(Context.ConnectionId, userId);
        }

        public void newMessage(Models.Chats chat, Models.Messages message)
        {
            Clients.Group(chat.Id.ToString()).newMessage(message);
        }
        public void newChat(Models.Chats chat, List<Models.Users> users)
        {
            foreach (var user in users)
            {
                foreach (var us in ChatHub.users.Where(e => e.Value == user.Id))
                {
                    Groups.Add(us.Key, chat.Id.ToString()).Wait();
                }
            }
            Clients.Group(chat.Id.ToString()).newChat(chat);
            
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