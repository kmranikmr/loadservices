using DataAnalyticsPlatform.Shared.DataModels;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.Models
{
    public class SingleRecord : IRecord
    {
        public object Instance { get; set; }//public Entity Instance { get; set; }//

        public SingleRecord(object instance)
        {
            Instance = instance;// (Entity)instance;
        }
    }
}