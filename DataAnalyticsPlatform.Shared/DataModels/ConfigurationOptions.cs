using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.DataModels
{
    public class ConnectionStringsConfig
    {
        public string DefaultConnection { get; set; }
        public string PostgresConnection { get; set; }
        public string ElasticSearchString { get; set; }

        public string MongoDBString { get; set; }
    }
}
