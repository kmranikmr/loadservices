using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Types;
using System;

namespace DataAnalyticsPlatform.Writers
{
    public class WriterConfiguration : IConfiguration
    {
        public int ProjectId { get; set; }

        public DestinationType DestinationType { get; set; }

        public string ConnectionString { get; set; }

        public string ConfigurationName { get; set; }

        public string SchemaName { get; set; }

        public Type ModelMap { get; set; }
        public WriterConfiguration()
        {

        }
        public WriterConfiguration(DestinationType destinationType, string connectionString, Type ModelMap)
        {
            DestinationType = destinationType;
            ConnectionString = connectionString;
            this.ModelMap = ModelMap;
        }
    }
}

