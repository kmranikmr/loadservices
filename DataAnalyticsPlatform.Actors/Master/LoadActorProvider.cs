using Akka.Actor;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DataAnalyticsPlatform.Actors.Cluster;

namespace DataAnalyticsPlatform.Actors.Master
{
    public class JobProgressHub : Hub
    {
        public async Task AssociateJob(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }
    public class LoadActorProvider
    {
        private IActorRef LoadInstance { get; set; }
        public PreviewRegistry previewRegistry { get; set; } 
        Notifier _notifier;
        private readonly IHubContext<JobProgressHub> _hubContext;
        public LoadActorProvider(ActorSystem actorSystem, PreviewRegistry previewRegistry, IHubContext<JobProgressHub> hubContext)
        {
            _hubContext = hubContext;
            _notifier = new Notifier();
            _notifier.OnNotification += new EventHandler<System.NotificationArgumet>(NotifyEnd);
            this.previewRegistry = previewRegistry;
            this.LoadInstance = actorSystem.ActorOf(Props.Create(() => new TaskRouterManager(_notifier, previewRegistry)), "tasker");//actorSystem.ActorOf(Props.Create(() => new MasterActor(_notifier, previewRegistry)), "masterNode");

        }
        public void NotifyEnd(object sender, System.NotificationArgumet ea)
        {
            _hubContext.Clients.Group(ea.Information).SendAsync("progress", "Done");
        }
        public IActorRef Get()
        {
            return this.LoadInstance;
        }
    }
}
