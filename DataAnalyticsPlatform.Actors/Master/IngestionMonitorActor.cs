using Akka.Actor;
using DataAnalyticsPlatform.Actors.Processors;
using DataAnalyticsPlatform.Shared.DataAccess;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Master
{
    public class IngestionMonitorActor : ReceiveActor
    {
        public string connectionString { get; set; }

        public IngestionMonitorActor()
        {
            //repository = new BulkPostgresRepository<BaseModel>(connectionString, "");
            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            Receive<CreateSchemaPostgres>(x =>
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(x.ConnectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format("create schema if not exists " + x.SchemaName);

                        command.ExecuteNonQuery();
                    }
                }
            });
        }
    }
}
