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
        public void connect(string accessToken)
        {
            var user = (Utils.CheckTokenResponse)((System.Web.Mvc.JsonResult)(new Controllers.TokensController().CheckToken(accessToken).Result)).Data;
            if (user != null)
            {
                foreach (var chatId in (List<long>)((System.Web.Mvc.JsonResult)(new Controllers.UsersController().GetChats(accessToken, 50, 0).Result)).Data)
                {
                    Groups.Add(Context.ConnectionId, chatId.ToString());
                }
                try
                {
                    users.Add(Context.ConnectionId, user.userId);
                }
                catch { }
            }
        }

        internal void newChat(Models.Chats chat, List<long> userIds)
        {
            if (chat.Type == Enums.ChatType.Dialog)
            {
                return;
            }
            foreach (var userId in userIds)
            {
                foreach (var user in users.Where(e => e.Value == userId))
                {
                    Groups.Add(user.Key, chat.Id.ToString()).Wait();
                }
            }
            Clients.Group(chat.Id.ToString()).newChat(new { id = chat.Id, creator = chat.Creator, name = chat.Name, avatar = chat.Avatar, type = chat.Type});
        }

        internal void newDialog(Models.Chats chat, long firstUser, long secondUser)
        {
            if (chat.Type != Enums.ChatType.Dialog)
            {
                return;
            }

            var names = chat.Name.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var user in users.Where(e => e.Value == firstUser))
            {
                Clients.Client(user.Key.ToString()).newChat(new { id = chat.Id, creator = chat.Creator, name = names[0], avatar = chat.Avatar, type = chat.Type });
            }
            foreach (var user in users.Where(e => e.Value == secondUser))
            {
                Clients.Client(user.Key.ToString()).newChat(new { id = chat.Id, creator = chat.Creator, name = names[1], avatar = chat.Avatar, type = chat.Type });
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