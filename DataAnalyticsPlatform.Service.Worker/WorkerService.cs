using Akka.Actor;
using Akka.Bootstrap.Docker;
using DataAnalyticsPlatform.Actors.Utils;
using DataAnalyticsPlatform.Actors.Worker;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Service.Worker
{
    public class WorkerService
    {
        protected IActorRef WorkerNode;

        protected ActorSystem DAPClusterSystem;

        public Task WhenTerminated => DAPClusterSystem.WhenTerminated;

        public bool Start()
        {
            var config = HoconLoader.ParseConfig("worker.hocon");
            DAPClusterSystem = ActorSystem.Create("dap-actor-system", config.BootstrapFromDocker());
            WorkerNode = DAPClusterSystem.ActorOf(Props.Create(() => new WorkerNode(1)), "WorkerNode");
            return true;
        }

        public Task Stop()
        {
            return CoordinatedShutdown.Get(DAPClusterSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }
    }
}
