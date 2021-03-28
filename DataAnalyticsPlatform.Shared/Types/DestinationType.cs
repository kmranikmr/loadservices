using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.Types
{
    public enum DestinationType
    {
        None,
        Mongo,
        ElasticSearch,
        csv,
        RDBMS
    }
}
