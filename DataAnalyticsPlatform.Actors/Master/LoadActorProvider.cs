/*
 * This file contains the JobProgressHub class and the LoadActorProvider class within the DataAnalyticsPlatform's Master actors namespace.
 * 
 * JobProgressHub:
 * - Inherits from Microsoft.AspNetCore.SignalR.Hub and provides a method AssociateJob(string jobId) to associate a SignalR connection with a specific job ID group.
 * - Uses SignalR's Groups.AddToGroupAsync method to add the current connection to a SignalR group identified by the provided job ID.
 * 
 * LoadActorProvider:
 * - Manages the creation and retrieval of an Akka.NET actor instance responsible for handling job loading tasks.
 * - Initializes with an ActorSystem, a PreviewRegistry instance, and a SignalR HubContext<JobProgressHub> for real-time job progress updates.
 * - Creates a TaskRouterManager actor instance using the provided ActorSystem, Notifier, and PreviewRegistry during instantiation.
 * - Registers an event handler to notify the SignalR group associated with a job ID when a job completes, using the OnNotification event of the Notifier.
 * - Exposes a method Get() to retrieve the initialized actor instance for external use.
 */


using Akka.Actor;
using DataAnalyticsPlatform.Actors.Cluster;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

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
            this.LoadInstance = actorSystem.ActorOf(Props.Create(() => new TaskRouterManager(_notifier, previewRegistry)), "tasker");

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
