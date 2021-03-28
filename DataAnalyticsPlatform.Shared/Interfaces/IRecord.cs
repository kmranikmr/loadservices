using DataAnalyticsPlatform.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.Interfaces
{
    public interface IRecord
    {
        object Instance { get; set; }
        long RecordId { get; set; }
        long FileId { get; set; }
        //Entity Instance { get; set; }
    }
}
