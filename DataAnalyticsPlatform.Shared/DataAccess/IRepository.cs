using System.Collections.Generic;

namespace DataAnalyticsPlatform.Shared.DataAccess
{
    public interface IRepository<TEntity>
    {
        //TEntity GetById(object id);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void Insert(TEntity e, string name = "");
        bool CreateTables(List<TEntity> models, string schema, bool dropTable = false);

        bool CreateSchema(string schemaName);

        bool ExecuteSqlQuery(string command);

        object ExecuteScalar(string commandText);

        // DataTable GetResultTable(string command);
        void Dispose();

    }

}
