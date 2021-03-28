using Akka.Actor;
using DataAnalyticsPlatform.Actors;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Registry
{
    public class RegistryActorProvider
    {
        private IActorRef RegistryActor { get; set; }
        private PreviewRegistry previewRegistry;
        public RegistryActorProvider(ActorSystem actorSystem , PreviewRegistry previewRegistry)
        {
            this.previewRegistry = previewRegistry;
            this.RegistryActor = actorSystem.ActorOf(Props.Create(() => new RegistryActor(previewRegistry)), "registry");

        }

        public IActorRef Get()
        {
            return this.PreviewsInstance;
        }

    }
}
