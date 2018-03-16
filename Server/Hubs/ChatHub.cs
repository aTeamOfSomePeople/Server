using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Server.Hubs
{
    [HubName("ZeroMessenger")]
    public class ChatHub : Hub
    {
        public void connect(List<Models.Chats> chats)
        {
            foreach(var chat in chats)
            {
                Groups.Add(Context.ConnectionId, chat.Id.ToString());
            }
        }
        public void newMessage(Models.Chats chat, Models.Messages message)
        {
            Clients.OthersInGroup(chat.Id.ToString()).newMessage(message);
        }
    }
}