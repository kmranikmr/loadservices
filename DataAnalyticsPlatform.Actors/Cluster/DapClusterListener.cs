using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using System;

namespace DataAnalyticsPlatform.Actors.Cluster
{
    // NLog for logging
    using NLog;

    /// <summary>
    /// Actor that listens for Akka.NET cluster events and logs them.
    /// </summary>
    public class DapClusterListener : ReceiveActor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);

        public static readonly String DEFAULT_NAME = "DapClusterListener";

        public static Akka.Actor.Props Props => Akka.Actor.Props.Create<DapClusterListener>();

        public DapClusterListener()
        {
            SetReceiveHandlers();
        }

        /// <summary>
        /// Sets up message handlers for cluster events.
        /// </summary>
        private void SetReceiveHandlers()
        {
            // Log when a member joins the cluster
            Receive<ClusterEvent.MemberUp>(message =>
            {
                logger.Info($"Member is Up: {message.Member}");
            });

            // Log when a member becomes unreachable
            Receive<ClusterEvent.UnreachableMember>(message =>
            {
                logger.Warn($"Member detected as unreachable: {message.Member}");
            });

            // Log when a member is removed
            Receive<ClusterEvent.MemberRemoved>(message =>
            {
                logger.Info($"Member is Removed: {message.Member}");
            });

            // Ignore other member events
            Receive<ClusterEvent.IMemberEvent>(message =>
            {
                // ignore
            });

            // Handle any other messages
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
        /// Unsubscribe from cluster events on actor stop.
        /// </summary>
        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }
    }
}
