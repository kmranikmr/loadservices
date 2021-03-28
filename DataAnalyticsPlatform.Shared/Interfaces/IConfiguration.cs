using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.Interfaces
{
    public interface IConfiguration
    {
        string ConfigurationName { get; set; }
    }
}
