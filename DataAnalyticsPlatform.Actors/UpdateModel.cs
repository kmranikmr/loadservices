using Akka.Actor;
using DataAnalyticsPlatform.Actors.Preview;
using DataAnalyticsPlatform.Common;
//using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.SharedUtils;
//using DataAnalyticsPlatform.SharedUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors
{
    public class UpdateModel
    {
        private IActorRef PreviewsActor { get; set; }

        public UpdateModel(PreviewActorProvider provider)
        {

            this.PreviewsActor = provider.Get();
        }

        public async Task<Tuple<string, Dictionary<string, List<BaseModel>>>> Execute(int userId, TypeConfig typeConfig, string FileName, string customConfiguration = "")
        {

            return await this.PreviewsActor.Ask<Tuple<string, Dictionary<string,List<BaseModel>>>>(new messages.PreviewActor.UpdateModel(userId, typeConfig, FileName, customConfiguration));
        }
    }
}
