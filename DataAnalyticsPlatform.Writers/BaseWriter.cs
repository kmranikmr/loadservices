using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.ExceptionUtils;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Types;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Writers
{
    public abstract class BaseWriter : IWriter
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public DestinationType DestinationType { get; set; }

        public BaseWriter(string connectionString, DestinationType destinationType)
        {
            ConnectionString = connectionString;
            DestinationType = destinationType;
        }

        public event EventHandler<ErrorArgument> OnError;

        public event EventHandler<InfoArgument> OnInfo;
        public abstract Dictionary<string, long?> DataSize();
        public abstract void Write(IRecord record);
        public abstract void Write(object record);
        public abstract void Write(List<object> record);
        public abstract void Write(List<BaseModel> record);
        public abstract void Dispose();
        public abstract bool CreateTables(List<object> model, string db, string schema, string table);
        public abstract bool CreateTables(List<BaseModel> model, string db, string schema, string table);

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
