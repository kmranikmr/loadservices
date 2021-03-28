using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadServiceApi
{
    public class LoadServiceProgressHub : Hub
    {
        public const string GROUP_NAME = "progress";

        public override Task OnConnectedAsync()
        {
            // https://github.com/aspnet/SignalR/issues/2200
            // https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/working-with-groups

            return Groups.AddToGroupAsync(Context.ConnectionId, "progress");

            // Clients.User(Context.User?.Identity?.Name).SendAsync("progress");
        }
    }
}
