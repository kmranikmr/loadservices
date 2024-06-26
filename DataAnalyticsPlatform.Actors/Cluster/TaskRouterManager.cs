
using Akka.Actor;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Shared.Models;
using System;


namespace DataAnalyticsPlatform.Actors.Cluster
{
    class TaskRouterManager : ReceiveActor
    {
        private const string RouterName = "taskrouter";
        protected IActorRef ProcessRouter;
        private ICancelable cancelTimer;
        public Notifier _notifier;
        public PreviewRegistry _previewRegistry;
        public TaskRouterManager(Notifier notifier, PreviewRegistry previewRegistry)
        {
            _notifier = notifier;
            _previewRegistry = previewRegistry;
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
            Receive<RouterCheck>(msg =>
            {
                var members = ProcessRouter.Ask<Routees>(new GetRoutees()).Result.Members;


                if (members.GetEnumerator().MoveNext() == false)
                {
                    Console.WriteLine("task router  has no routees. Waiting.");


                    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new RouterCheck(), Self);
                    return;
                }


                Become(Ready);
            });


            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new RouterCheck(), Self);
        }
        private void Ready()
        {
            Receive<IngestionJob>(x =>
            {
                ProcessRouter.Tell(new JobRegistry(x, _previewRegistry));
            });
        }
    }

    internal class RouterCheck
    {
        public RouterCheck()
        {
        }
    }
}
