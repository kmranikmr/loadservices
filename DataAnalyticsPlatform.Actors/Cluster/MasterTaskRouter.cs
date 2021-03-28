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
{
    public class MasterTaskRouter : ReceiveActor
    {
        private const string RouterName = "mastertaskrouter";
        protected IActorRef ProcessRouter;
        private ICancelable cancelTimer;

        public MasterTaskRouter()
        {
            WaitForRoutees();
        }

        protected override void PreStart()
        {
            ProcessRouter = Context.Child(RouterName).Equals(ActorRefs.Nobody)
                ? Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), RouterName)
                : Context.Child(RouterName);
        }

        private void WaitForRoutees()
        {
            Receive<EnsureRouteesAreUp>(msg =>
            {
                var members = ProcessRouter.Ask<Routees>(new GetRoutees()).Result.Members;
                if (members.GetEnumerator().MoveNext() == false)
                {
                    Console.WriteLine("Round robin router has no routees. Waiting.");

                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new EnsureRouteesAreUp(), Self);
                    return;
                }


                Become(Ready);
            });

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new EnsureRouteesAreUp(), Self);
        }
        private void Ready()
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
