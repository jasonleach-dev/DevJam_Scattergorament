using DevJamServer.Worker;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevJamServer.Hubs
{
    public class ScattergoramentHub:Hub
    {
        public void RegisterPlayer()
        {
            Scattergorament.Instance.AddPlayer(Context.ConnectionId);
            Scattergorament.Instance.SendStatusUpdate(this.Clients.Caller);
        }

        public void SetGuess(string guess)
        {
            Scattergorament.Instance.SetGuess(Context.ConnectionId, guess);
        }
    }
}