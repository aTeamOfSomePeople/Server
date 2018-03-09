using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Server
{
    public class ServerHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}