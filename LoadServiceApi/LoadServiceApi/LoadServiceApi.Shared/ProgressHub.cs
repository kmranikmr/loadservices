using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LoadServiceApi
{
    public class ProgressHub : Hub
    {
        public const string GROUP_NAME = "processing";

        public override Task OnConnectedAsync()
        {
            // https://github.com/aspnet/SignalR/issues/2200
            // https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/working-with-groups

            return Groups.AddToGroupAsync(Context.ConnectionId, "processing");

            // Clients.User(Context.User?.Identity?.Name).SendAsync("progress");
        }
    }
}
