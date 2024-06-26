using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Preview
{
    public class GetModels
    {
        private IActorRef PreviewsActor { get; set; }

        public GetModels(PreviewActorProvider provider)
        {
            // this.Logger = logger;
            this.PreviewsActor = provider.Get();
        }

        public async Task<SchemaModels> Execute(int userId)
        {
            // Logger.LogInformation($"Requesting model of user '{userId}'");
            return await this.PreviewsActor.Ask<SchemaModels>(new messages.PreviewActor.GetModel(userId));
        }
    }
}
