using DataAnalyticsPlatform.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Master
{
    public class JobFeed
    {
        public int UserId { get; set; }
        public string JobId { get; set; }
        public string ConnectionString { get; set; }
        public SourceType JobType { get; set; }
        public string SourcePath { get; set; }
    }
}
