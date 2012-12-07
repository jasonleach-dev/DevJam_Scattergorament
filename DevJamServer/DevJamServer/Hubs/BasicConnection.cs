using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevJamServer.Hubs
{
    public class BasicConnection:PersistentConnection
    {
        protected override System.Threading.Tasks.Task OnConnectedAsync(IRequest request, string connectionId)
        {
            return Connection.Broadcast("Connection " + connectionId + " connected");
        }
    }
}