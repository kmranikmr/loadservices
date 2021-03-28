using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.ExceptionUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.Interfaces
{
    public interface IWriter
    {
        string ConnectionString { get; set; }
        string SchemaName { get; set; }
        void Write(IRecord record);
        void Write(object record);
        void Write(List<object> record);
        void Write(List<BaseModel> record);
        bool CreateTables(List<object> model, string db, string schema, string table);
        bool CreateTables(List<BaseModel> model, string db, string schema, string table);
        void Dispose();
        event EventHandler<ErrorArgument> OnError;

        event EventHandler<InfoArgument> OnInfo;
    }
}
