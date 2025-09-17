using Akka.Actor;
using DataAnalyticsPlatform.Actors.Processors;
using Npgsql;

namespace DataAnalyticsPlatform.Actors.Master
    // NLog for logging
    using NLog;
{
    public class IngestionMonitorActor : ReceiveActor
    /// <summary>
    /// Logger instance for this class.
    /// </summary>
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    {
        public string connectionString { get; set; }

        public IngestionMonitorActor()
        {
            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            // Handle schema creation requests for PostgreSQL
            Receive<CreateSchemaPostgres>(x =>
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(x.ConnectionString))
                    {
                        connection.Open();
                        using (NpgsqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = string.Format("create schema if not exists " + x.SchemaName);
                            command.ExecuteNonQuery();
                            logger.Info($"Schema '{x.SchemaName}' created or already exists in PostgreSQL.");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Error(ex, $"Error creating schema '{x.SchemaName}' in PostgreSQL.");
                }
            });
        }
    }
}
