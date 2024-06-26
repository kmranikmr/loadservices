using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using System;

namespace DataAnalyticsPlatform.Actors.Cluster
{
    public class DapClusterListener : ReceiveActor
    {
        protected ILoggingAdapter Log = Context.GetLogger();

        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);

        public static readonly String DEFAULT_NAME = "DapClusterListener";

        public static Akka.Actor.Props Props => Akka.Actor.Props.Create<DapClusterListener>();

        public DapClusterListener()
        {
            SetReceiveHandlers();
        }

        private void SetReceiveHandlers()
        {
            Receive<ClusterEvent.MemberUp>(message =>
            {
                Log.Info("Member is Up: {0}", message.Member);
            });

            Receive<ClusterEvent.UnreachableMember>(message =>
            {
                Log.Info("Member detected as unreachable: {0}", message.Member);
            });

            Receive<ClusterEvent.MemberRemoved>(message =>
            {
                Log.Info("Member is Removed: {0}", message.Member);
            });

            Receive<ClusterEvent.IMemberEvent>(message =>
            {
                //ignore
            });

            ReceiveAny(message =>
            {
                Unhandled(message);
            });
        }



        /// <summary>
        /// Need to subscribe to cluster changes
        /// </summary>
        protected override void PreStart()
        {
            Cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents, new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.UnreachableMember) });
        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }
    }
}
