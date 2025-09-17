
using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Preview
{
    /// <summary>
    /// Handles model generation requests and communicates with PreviewActor.
    /// </summary>
    public class GenerateModel
    // NLog logger for logging events and debugging
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    {
        private IActorRef PreviewsActor { get; set; }

        public GenerateModel(PreviewActorProvider provider)
        {

            this.PreviewsActor = provider.Get();
        }

        public async Task<SchemaModel> Execute(int userId, string fileName, string readerConfiuration = "", int jobId = 0)
        {
            // Log the reader configuration being used for model generation
            logger.Info($"readerConfiuration: {readerConfiuration}");
            return await this.PreviewsActor.Ask<SchemaModel>(new messages.PreviewActor.GenerateModel(userId, fileName, readerConfiuration, jobId));
        }
    }
}
