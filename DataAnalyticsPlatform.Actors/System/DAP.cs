using Akka.Actor;
using DataAnalyticsPlatform.Actors.Master;
using System;

namespace DataAnalyticsPlatform.Actors.System
{
    public class DAP
    {
        ActorSystem _system = null;

        IActorRef _masterActor;

        Notifier _notifier;

        public DAP()
        {
            _system = ActorSystem.Create("dap-actor-system");

            _notifier = new Notifier();

            _notifier.OnNotification += _notifier_OnNotification;

            _masterActor = _system.ActorOf(Props.Create(() => new Master.MasterActor(_notifier, null)), "MasterActor");
        }

        private void _notifier_OnNotification(object sender, NotificationArgumet e)
        {
            Console.WriteLine(e.Information);
        }

        public void Feed(IngestionJob job)
        {
            _masterActor.Tell(job);
        }

        public void Dispose()
        {
            _system.Stop(_masterActor);
            _system.Dispose();
        }
    }
}
