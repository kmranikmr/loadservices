/*LoadModels:
 *-Manages the execution of ingestion jobs based on provided parameters like user ID, file name, type configurations, etc.
 * - Provides functionality to create database schemas if they do not exist using Npgsql.
 *-Uses Akka.NET actors managed by LoadActorProvider to distribute and manage ingestion tasks.
 * - Supports ingestion from CSV and JSON files, configuring reader and writer configurations accordingly.
 * - Handles special configurations for Twitter data and various destination types such as RDBMS and Elasticsearch.
 * - Initializes ingestion jobs using IngestionJob instances and communicates with Akka actors to process the jobs asynchronously.
 * - Retrieves necessary configuration details from a PreviewRegistry instance provided during initialization.
 * - Integrates with external systems like PostgreSQL, Elasticsearch, and MongoDB based on provided connection strings and configurations.
 */

using Akka.Actor;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Writers;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace DataAnalyticsPlatform.Actors.Master
{
    /// <summary>
    /// Manages the execution of ingestion jobs and schema creation for various data sources.
    /// </summary>
    public class LoadModels
    {
    /// <summary>
    /// Reference to the Akka.NET actor responsible for job execution.
    /// </summary>
    private IActorRef LoadActor { get; set; }
    /// <summary>
    /// The current ingestion job being processed.
    /// </summary>
    private IngestionJob _ingestionJob;
    /// <summary>
    /// Registry containing preview configuration and schema models.
    /// </summary>
    private PreviewRegistry previewRegsitry;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of LoadModels with the provided actor provider.
        /// </summary>
        /// <param name="provider">Provider for Akka.NET actor and preview registry.</param>
        public LoadModels(LoadActorProvider provider)
        {
            this.previewRegsitry = provider.previewRegistry;
            this.LoadActor = provider.Get();
        }
        /// <summary>
        /// Creates a schema in PostgreSQL if it does not exist.
        /// </summary>
        /// <param name="connectionString">Connection string for PostgreSQL.</param>
        /// <param name="schemaName">Name of the schema to create.</param>
        /// <returns>True if schema creation succeeds, false otherwise.</returns>
        public bool CreateSchema(string connectionString, string schemaName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        // Create schema if it does not exist
                        command.CommandText = string.Format("create schema if not exists " + schemaName);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exception as needed
                    return false;
                }
                return true;
            }
        }

        }


        /// <summary>
        /// Executes an ingestion job based on file type and configuration.
        /// </summary>
        /// <param name="userId">User ID for the job.</param>
        /// <param name="FileName">Name of the file to ingest.</param>
        /// <param name="typeConfigList">List of type configurations.</param>
        /// <param name="FileId">File ID.</param>
        /// <param name="jobId">Job ID.</param>
        /// <param name="configuration">Additional configuration in JSON format.</param>
        /// <param name="projectId">Project ID.</param>
        /// <param name="connectionString">General connection string.</param>
        /// <param name="postgresConnString">PostgreSQL connection string.</param>
        /// <param name="writers">Array of writer configurations.</param>
        /// <param name="elasticSearchString">Elasticsearch connection string.</param>
        /// <param name="mongoDBString">MongoDB connection string.</param>
        /// <returns>Returns 1 if successful, -1 if configuration is invalid.</returns>
        public async Task<int> Execute(
            int userId,
            string fileName,
            List<TypeConfig> typeConfigList,
            int fileId = 1,
            int jobId = 1,
            string configuration = "",
            int projectId = -1,
            string connectionString = "",
            string postgresConnString = "",
            DataAccess.Models.Writer[] writers = null,
            string elasticSearchString = "",
            string mongoDBString = "")
        {
            // Validate input
            if (string.IsNullOrEmpty(fileName) || typeConfigList == null || typeConfigList.Count == 0 || writers == null)
                return -1;

            // Retrieve schema models for the user
            var schemaModels = previewRegsitry.GetFromRegistry(userId);

            // Determine source type
            SourceType sourceType = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? SourceType.Csv
                : SourceType.Json;

            // Build reader configuration
            var readerConfig = new ReaderConfiguration(typeConfigList[0], fileName, sourceType, fileId)
            {
                ProjectId = projectId != -1 ? projectId : (int?)null
            };

            // Deserialize configuration details if provided
            if (!string.IsNullOrEmpty(configuration))
            {
                if (sourceType == SourceType.Csv)
                {
                    readerConfig.ConfigurationDetails = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                }
                else if (fileName.Contains("twitter", StringComparison.OrdinalIgnoreCase))
                {
                    readerConfig.ConfigurationDetails = JsonConvert.DeserializeObject<TwitterConfiguration>(configuration);
                }
            }

            // Build writer configurations
            var writerConfigs = new List<WriterConfiguration>();
            foreach (var writer in writers)
            {
                WriterConfiguration writerConfig = null;
                switch (writer.WriterTypeId)
                {
                    case 4: // Elasticsearch
                        writerConfig = new WriterConfiguration(DestinationType.ElasticSearch, elasticSearchString, null);
                        break;
                    case 3: // MongoDB
                        writerConfig = new WriterConfiguration(DestinationType.Mongo, mongoDBString, null);
                        break;
                    default: // RDBMS
                        writerConfig = new WriterConfiguration(DestinationType.RDBMS, postgresConnString, null);
                        var schemaName = readerConfig.TypeConfig.SchemaName.Replace(" ", string.Empty);
                        if (projectId != -1)
                            schemaName += $"_{projectId}_{userId}";
                        CreateSchema(postgresConnString, schemaName);
                        break;
                }
                if (projectId != -1)
                    writerConfig.ProjectId = projectId;
                writerConfigs.Add(writerConfig);
            }

            // Initialize and send ingestion job to actor
            _ingestionJob = new IngestionJob(jobId, readerConfig, writerConfigs.ToArray())
            {
                ControlTableConnectionString = connectionString,
                UserId = userId
            };
            LoadActor.Tell(_ingestionJob);

            // Run the ingestion job asynchronously
            return await Task.FromResult(1);
        }
    }
}

