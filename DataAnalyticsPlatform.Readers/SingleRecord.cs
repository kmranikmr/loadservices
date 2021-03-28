using DataAnalyticsPlatform.Shared.DataModels;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Readers
{
    public partial class SingleRecord : IRecord
    {
        public Entity Instance { get ; set ; }

        public SingleRecord(object instance)
        {
            Instance = (Entity)instance;
        }
    }
}
