using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Types;
using System;

namespace DataAnalyticsPlatform.Writers
{
    public static class Factory
    {
        public static IWriter GetWriter(WriterConfiguration conf)
        {
            switch (conf.DestinationType)
            {
                case DestinationType.ElasticSearch:
                    return new ElasticWriter(conf.ConnectionString);
                case DestinationType.Mongo:
                    return new MongoWriter(conf.ConnectionString);
                case DestinationType.csv:
                    return new Csvwriter(conf);
                case DestinationType.RDBMS:
                    return new RDBMSBulkWriter(conf.ConnectionString, conf.SchemaName);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
