using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Registry
{
    public class GetAllRegistry
    {
        private IActorRef RegistryActor { get; set; }

        public GetAllRegistry(RegistryActorProvider provider)
        {
            // this.Logger = logger;
            this.RegistryActor = provider.Get();
        }

        public async Task<SchemaModels> Execute(int userId)
        {
            // Logger.LogInformation($"Requesting model of user '{userId}'");
            return await this.PreviewsActor.Ask<SchemaModels>(new messages.PreviewActor.GetModel(userId));
        }
    }
}
