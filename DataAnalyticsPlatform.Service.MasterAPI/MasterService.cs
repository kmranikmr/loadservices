using Akka.Actor;
using Akka.Bootstrap.Docker;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.Utils;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Service.MasterAPI
{
    public class MasterService
    {
        protected IActorRef MasterActor;

        protected ActorSystem DAPClusterSystem;
        protected IActorRef MasterRouter;
        public string ConnectionString { get; set; }
        public string PostgresString { get; set; }

        public string ElasticString { get; set; }

        public string MongoString { get; set; }
        public Task WhenTerminated => DAPClusterSystem.WhenTerminated;

        public bool Start()
        {
            var config = HoconLoader.ParseConfig("master.hocon");

            ConnectionString = GetAppStringSetting("DbConnectionString", null);
            PostgresString = GetAppStringSetting("Postgres", null);
            ElasticString = GetAppStringSetting("Elastic", null);
            MongoString = GetAppStringSetting("Mongo", null);
            DAPClusterSystem = ActorSystem.Create("dap-actor-system", config.BootstrapFromDocker());//config.BootstrapFromDocker()
            SpawnMaster();
            return true;
        }


        private string GetAppStringSetting(string key, string defaultValue = null)
        {
            try
            {
                string value = null;

                if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                {
                    value = ConfigurationManager.AppSettings[key];

                    if (value != null) return value.ToString();
                }

                return value ?? defaultValue;
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
            }

            return string.Empty;
        }

        public bool SpawnMaster()
        {
            //  this.MasterRouter = DAPClusterSystem.ActorOf(Props.Create(() => new MasterTaskRouter()), "masterTaskRouter");//acto
            string el = ElasticString;// 
            MasterActor = DAPClusterSystem.ActorOf(Props.Create(() => new MasterActor(null, ConnectionString, PostgresString, el, MongoString)), "masterNode");


            return true;
        }

        public Task Stop()
        {
            return CoordinatedShutdown.Get(DAPClusterSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }
    }
}
