using DataAnalyticsPlatform.Shared.ExceptionUtils;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Data;

namespace DataAnalyticsPlatform.Readers
{
    public abstract class BaseReader : IReader
    {
        public IConfiguration Configuration { get; set; }

        public BaseReader(ReaderConfiguration conf)
        {
            Configuration = conf;
        }

        public abstract DataTable Preview(int size);
        public abstract bool GetRecords(out IRecord record);
        public abstract bool GetRecords(out IRecord record, Type type);

        public event EventHandler<ErrorArgument> OnError;

        public event EventHandler<InfoArgument> OnInfo;

        public ReaderConfiguration GetConfiguration()
        {
            return (ReaderConfiguration)Configuration;
        }

        public void LogError(string errorMessage)
        {
            OnError?.Invoke(this, new ErrorArgument() { ErrorMessage = errorMessage });
        }

        public void LogInfo(string information)
        {
            OnInfo?.Invoke(this, new InfoArgument() { Information = information });
        }
    }
}
