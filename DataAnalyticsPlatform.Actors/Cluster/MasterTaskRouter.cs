using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Text;
using Akka;
using Akka.Actor;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Shared.Models;

namespace DataAnalyticsPlatform.Actors.Cluster
    // NLog for logging
    using NLog;
{
    public class MasterTaskRouter : ReceiveActor
    /// <summary>
    /// Logger instance for this class.
    /// </summary>
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    {
        private const string RouterName = "mastertaskrouter";
        protected IActorRef ProcessRouter;
        private ICancelable cancelTimer;

        public MasterTaskRouter()
            // Start waiting for routees to be available
        {
            WaitForRoutees();
        }

        protected override void PreStart()
            // Initialize the router actor, creating it if it doesn't exist
        {
            ProcessRouter = Context.Child(RouterName).Equals(ActorRefs.Nobody)
                ? Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), RouterName)
                : Context.Child(RouterName);
        }

        private void WaitForRoutees()
        {
            // Check if routees are available for the router
            Receive<EnsureRouteesAreUp>(msg =>
            {
                var members = ProcessRouter.Ask<Routees>(new GetRoutees()).Result.Members;
                if (members.GetEnumerator().MoveNext() == false)
                {
                    logger.Warn("Round robin router has no routees. Waiting.");
                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new EnsureRouteesAreUp(), Self);
                    return;
                }
                // Routees are available, switch to Ready behavior
                Become(Ready);
            });
            // Schedule initial routee check
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new EnsureRouteesAreUp(), Self);
        }
        private void Ready()
            // When ready, handle incoming ingestion jobs
        {
            Receive<IngestionJob>(x =>
            {
                ProcessRouter.Tell(x);
            });
        }
    }

    internal class EnsureRouteesAreUp
    {
    }
}
