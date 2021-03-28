using Akka.Actor;
using DataAnalyticsPlatform.Actors;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Preview
{
    public class PreviewActorProvider
    {
        private IActorRef PreviewsInstance { get; set; }
        private PreviewRegistry previewRegistry;
        public PreviewActorProvider(ActorSystem actorSystem , PreviewRegistry previewRegistry)
        {
            this.previewRegistry = previewRegistry;
            this.PreviewsInstance = actorSystem.ActorOf(Props.Create(() => new PreviewsActor(previewRegistry)), "previews");

        }

        public IActorRef Get()
        {
            return this.PreviewsInstance;
        }
         
    }
}
