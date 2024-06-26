
using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Preview
{
    public class GenerateModel
    {
        private IActorRef PreviewsActor { get; set; }

        public GenerateModel(PreviewActorProvider provider)
        {

            this.PreviewsActor = provider.Get();
        }

        public async Task<SchemaModel> Execute(int userId, string fileName, string readerConfiuration = "", int jobId = 0)
        {
            Console.WriteLine("readerConfiuration " + readerConfiuration);
            return await this.PreviewsActor.Ask<SchemaModel>(new messages.PreviewActor.GenerateModel(userId, fileName, readerConfiuration, jobId));
        }
    }
}
