using DataAnalyticsPlatform.Shared.ExceptionUtils;
using System;
using System.Data;

namespace DataAnalyticsPlatform.Shared.Interfaces
{
    public interface IReader
    {
        DataTable Preview(int size);

        IConfiguration Configuration { get; set; }

        bool GetRecords(out IRecord record);
        bool GetRecords(out IRecord record, Type type);
        event EventHandler<ErrorArgument> OnError;

        event EventHandler<InfoArgument> OnInfo;
    }
}
