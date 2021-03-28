using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadService
{
    public class ProgressHub : Hub
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
