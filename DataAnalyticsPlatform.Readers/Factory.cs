using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Readers
{
    public static class Factory
    {
        public static IReader GetReader(ReaderConfiguration conf)
        {
            switch (conf.SourceType)
            {
                case SourceType.Csv:
                    return new CsvReader(conf);
                case SourceType.Json:
                    if ( conf.ConfigurationDetails != null && conf.ConfigurationDetails is TwitterConfiguration)
                        return new TwitterReader(conf, conf.Types);
                    else
                        return new JsonReader(conf);
               
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
