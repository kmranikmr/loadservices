using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;

namespace DataAnalyticsPlatform.Actors.Preview
{
    public class PreviewActorProvider
    {
        private IActorRef PreviewsInstance { get; set; }
        private PreviewRegistry previewRegistry;
        public PreviewActorProvider(ActorSystem actorSystem, PreviewRegistry previewRegistry)
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
