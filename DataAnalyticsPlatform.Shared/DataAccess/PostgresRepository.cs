/// <summary>
/// The PgRepository<TEntity> class provides a comprehensive data access layer for interacting with a PostgreSQL database using Npgsql.
/// It supports operations such as creating schemas and tables, inserting entities with batch processing, and handling data type conversions.
/// 
/// Key functionalities include:
/// 
/// - Initialization and Connection Management: Establishes a connection to the PostgreSQL database using the provided connection string.
///   Supports schema management and maintains a dictionary of created tables and commands for efficient reuse.
/// 
/// - Schema and Table Creation: Dynamically creates schemas and tables based on the entity's properties. Includes handling for different data types.
/// 
/// - Insertion: Handles entity insertion with support for batching to improve performance. Uses parameterized queries to avoid SQL injection.
/// 
/// - Transaction Management: Supports transaction handling for batch operations to ensure data consistency.
/// 
/// - Utility Methods: Provides utility methods for executing scalar queries and SQL commands.
/// 
/// - Disposal: Ensures proper disposal of database connections and transactions.
/// </summary>

using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataAnalyticsPlatform.Shared.DataAccess
{
    public partial class PgRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private NpgsqlConnection _connection;

        private readonly Dictionary<string, string> _TablesTobeCreated;

        private NpgsqlTransaction _transaction;

        private int _counter;

        private int _batchSize = 100000;

        private bool isConnected = false;

        private string _connectionString;



        public string schema { get; set; }
        Dictionary<string, NpgsqlCommand> Dictcommand = null;
        NpgsqlCommand command = null;
        public Dictionary<string, bool> CreatedTable = null;
        public bool haveCreated = false;
        public PgRepository(string connectionString, string schemaName = "")
        {
            try
            {
                CreatedTable = new Dictionary<string, bool>();
                _connectionString = connectionString;
                _connection = new NpgsqlConnection(connectionString);
                _connection.Open();
                haveCreated = false;
                _TablesTobeCreated = new Dictionary<string, string>();
                if (schemaName != "")
                    schema = schemaName;
                else
                    schema = "public";
                Dictcommand = new Dictionary<string, NpgsqlCommand>();
            }
            catch (Exception ex)
            {
                int g = 0;
            }
        }
        public void Delete(TEntity entity)
        {

        }

        public bool CreateSchema(string schemaName)
        {
            // " create schema if not" present
            schema = schemaName;
            string sql = "create schema if not exists " + schemaName;
            ExecuteScalar(sql);

            using (StreamReader sr = new StreamReader(@"Scripts\PostgresFunctions.txt"))
            {
                string functions = sr.ReadToEnd();
                functions = functions.Replace("schemaholder", schemaName);
                ExecuteSqlQuery(functions);
            }
            return false;
        }
        public bool CreateTables(List<TEntity> models, string schema, bool dropTable = false)
        {
            try
            {
                Console.WriteLine("create tables " + schema);
                int i = 0;
                if (schema != "")
                    this.schema = schema;

                //for (int i = 0; i < models.Count; i++)
                {
                    var props = models[i].GetType().GetProperties();

                    var paramNames = new List<string>();

                    var columnNames = new List<string>();

                    var columnNameWithdataTypes = new List<string>();

                    var createTableCommand = _connection.CreateCommand();

                    var tableNameAttr = models[i].GetType();

                    var tableName = tableNameAttr != null ? tableNameAttr.Name : "";

                    Console.WriteLine("create tables " + tableName);

                    if (CreatedTable.TryGetValue(tableName, out haveCreated))
                    {
                        if (haveCreated)
                            return false;
                    }
                    Console.WriteLine("create tables 2" + tableName);
                    for (int j = props.Count() - 1; j >= 0; j--) //var propertyInfo in props)
                    {

                        var columnNameAttr = props[j].Name;
                        var columnName = columnNameAttr;

                        object value = props[j].GetValue(models[i]);

                        var paramName = string.Format("@Param_{0}", columnName);

                        columnNames.Add(string.Format(@"{0}", columnName));

                        paramNames.Add(paramName);

                        string propWithDataType = null;

                        var parameter = GetParameter(columnName, props[j].PropertyType, value, j, ref propWithDataType);

                        columnNameWithdataTypes.Add(string.Format(@"{0} {1}", columnName, propWithDataType));
                    }

                    // if (dropTable)
                    {
                        //createTableCommand.CommandText = string.Format("drop table if exists " + this.schema + ".{0};", tableName);
                        //createTableCommand.ExecuteNonQuery();
                        Console.WriteLine("create tables start" + tableName);
                        createTableCommand.CommandText = string.Format("create schema if not exists " + this.schema);
                        createTableCommand.ExecuteNonQuery();

                        createTableCommand.CommandText = string.Format("Create table if not exists " + this.schema + ".{0}({1});", tableName,
                              string.Join(",", columnNameWithdataTypes));

                        createTableCommand.ExecuteNonQuery();
                        Console.WriteLine("create tables done" + createTableCommand.CommandText);
                    }
                    //haveCreated = true;
                    if (!CreatedTable.TryGetValue(tableName, out haveCreated))
                    {
                        CreatedTable.Add(tableName, true);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return true;
        }
        private NpgsqlParameter GetParameter(string columnName, Type propType, object value, int index,
        ref string dataType)
        {
            var parameter = new NpgsqlParameter
            {
                ParameterName = "@Param_" + columnName,
                NpgsqlDbType = NpgsqlDbType.Integer,
                Value = DBNull.Value
            };

            if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                Type t = Nullable.GetUnderlyingType(propType);
                propType = t;
                parameter.IsNullable = true;
            }

            dataType = parameter.NpgsqlDbType.ToString();

            if (propType == typeof(SByte))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Smallint;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToInt16(value);
            }
            else if (propType == typeof(Byte))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Smallint;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToInt16(value);
            }
            else if (propType == typeof(Int64))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Bigint;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(Int32))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Integer;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(Int16))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Smallint;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(UInt16))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Smallint;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToInt16(value);
            }
            else if (propType == typeof(UInt32))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Integer;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToInt32(value);
            }
            else if (propType == typeof(double))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Real;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(Single))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Real;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToDouble(value);
            }
            else if (propType == typeof(string))
            {
                if (value == null || (string.IsNullOrEmpty(value.ToString())))
                {
                    parameter.NpgsqlDbType = NpgsqlDbType.Varchar;
                    dataType = parameter.NpgsqlDbType.ToString();
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.NpgsqlDbType = NpgsqlDbType.Varchar;
                    dataType = parameter.NpgsqlDbType.ToString();
                    parameter.Value = value;
                }
            }
            else if (propType == typeof(DateTime))
            {

                parameter.NpgsqlDbType = NpgsqlDbType.Timestamp;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(System.Decimal))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Numeric;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = Convert.ToDecimal(value);
            }
            else if (propType == typeof(byte[]))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Bytea;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(Boolean))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Boolean;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = value;
            }
            else if (propType == typeof(String[]) || propType == typeof(string[]))
            {
                if (value == null || (string.IsNullOrEmpty(value.ToString())))
                {
                    parameter.NpgsqlDbType = NpgsqlDbType.Text;
                    dataType = parameter.NpgsqlDbType.ToString();
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.NpgsqlDbType = NpgsqlDbType.Text;
                    dataType = parameter.NpgsqlDbType.ToString();
                    parameter.Value = string.Join(",", (string[])value);
                }
            }
            else if (propType == typeof(Single[]))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Text;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = string.Join(",", (Single[])value); ;
            }
            else if (propType == typeof(Int32[]))
            {
                parameter.NpgsqlDbType = NpgsqlDbType.Array;
                dataType = parameter.NpgsqlDbType.ToString();
                parameter.Value = (Int32[])value;
            }
            else
            {
                if (propType.GetTypeInfo().GetType() != typeof(int))
                {
                    parameter.NpgsqlDbType = NpgsqlDbType.Text;
                    dataType = parameter.NpgsqlDbType.ToString();
                    parameter.Value = JsonConvert.SerializeObject(propType.GetTypeInfo().GetType());
                }
            }
            //else if (type == typeof(MongoDB.Bson.ObjectId))
            //{
            //    dataType = "Char(24) PRIMARY KEY NOT NULL";
            //    parameter.NpgsqlDbType = NpgsqlDbType.Char;
            //    parameter.Value = ((MongoDB.Bson.ObjectId)value).ToString();
            //}
            //else if (type == typeof(Nemo.Analyze.Types.SystemData))
            //{
            //    parameter.NpgsqlDbType = NpgsqlDbType.Json;
            //    dataType = parameter.NpgsqlDbType.ToString();
            //    parameter.Value = JsonConvert.SerializeObject(value);
            //}

            return parameter;
        }
        //public TEntity GetById(TKey id)
        //{
        //    return default(TEntity);
        //}

        public void Insert(TEntity entity, string name = "")
        {
            try
            {
                BeginTransaction();//wwe need faster insertion - this poc


                if (string.IsNullOrEmpty(name))
                {
                    var tableNameAttr = entity.GetType();

                    name = tableNameAttr != null ? tableNameAttr.Name : "";
                }
                bool haveCommand = false;
                if (!Dictcommand.TryGetValue(name, out command))
                {
                    if (command == null)
                    {
                        command = GetInsertCommand(entity, _connection);

                        command.Connection = _connection;
                    }

                    Dictcommand.Add(name, command);
                }

                {
                    var props = entity.GetType().GetProperties();

                    int index = 0;

                    foreach (var propertyInfo in props)
                    {
                        var type = propertyInfo.PropertyType;

                        var columnNameAttr = propertyInfo.Name;

                        var columnName = columnNameAttr;// columnNameAttr != null ? columnNameAttr.Name : "";

                        var paramName = "@Param_" + columnName;

                        var value = propertyInfo.GetValue(entity) ?? DBNull.Value;
                        Console.WriteLine("prop info : " + paramName + " value " + value);
                        var parameter = command.Parameters[paramName];

                        if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            if (value == null || value == DBNull.Value)
                            {
                                parameter.Value = DBNull.Value;
                            }
                            else
                            {
                                Type t = Nullable.GetUnderlyingType(type);
                                parameter.Value = Convert.ChangeType(value, t);
                            }
                        }
                        else if (type == typeof(SByte))
                        {
                            parameter.Value = Convert.ToInt16(value);
                        }
                        else if (type == typeof(Byte))
                        {
                            parameter.Value = Convert.ToInt16(value);
                        }
                        else if (type == typeof(String[]) || type == typeof(string[]))
                        {
                            if (value != DBNull.Value)
                                parameter.Value = string.Join(",", (string[])value);
                        }
                        else if (type == typeof(UInt16))
                        {
                            parameter.Value = Convert.ToInt16(value);
                        }

                        else if (type == typeof(Int32))
                        {
                            parameter.Value = Convert.ToInt32(value);
                        }
                        else if (type == typeof(UInt32))
                        {
                            parameter.Value = Convert.ToInt32(value);
                        }
                        else if (type == typeof(Int64))
                        {
                            parameter.Value = value != DBNull.Value ? Convert.ToInt64(value) : 0;
                        }
                        else if (type == typeof(System.Decimal))
                        {
                            parameter.Value = Convert.ToDecimal(value);
                        }
                        else if (type == typeof(string))
                        {
                            if (string.IsNullOrEmpty(value.ToString()))
                            {
                                parameter.Value = DBNull.Value;
                            }
                            else
                            {
                                parameter.Value = value;
                            }

                        }
                        else if (type == typeof(Single[]))
                        {
                            parameter.Value = string.Join(",", (Single[])value);
                        }
                        //else if (type == typeof(MongoDB.Bson.ObjectId))
                        //{
                        //    parameter.Value = ((MongoDB.Bson.ObjectId)value).ToString();
                        //}
                        //else if (type == typeof(Nemo.Analyze.Types.SystemData))
                        //{
                        //    parameter.Value = JsonConvert.SerializeObject(value);
                        //}
                        else
                        {
                            // if (type == typeof(object))
                            //  {
                            parameter.Value = JsonConvert.SerializeObject(value);
                            //}
                            //else
                            //{
                            //    parameter.Value = value;
                            //}
                        }
                    }
                }

                command.Transaction = _transaction;

                command.Connection = _connection;

                command.ExecuteNonQuery();

                _counter++;

                if ((_counter % _batchSize == 0))
                {
                    Commit();
                }
                command = null;
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void BeginTransaction()
        {
            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            }
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
                _counter = 0;
            }
        }
        public void Dispose()
        {
            if (_counter > 0 && _transaction != null)
            {
                Commit();
            }

            command = null;
            //  _eventNameToDbCommand.Clear();

            if (_connection != null)
            {
                _connection.Close();

                _connection.Dispose();
            }

            //Console.WriteLine("Postgres connection disposed.");
        }

        private NpgsqlCommand GetInsertCommand(TEntity entity, NpgsqlConnection connection)
        {
            var command = new NpgsqlCommand();

            var props = entity.GetType().GetProperties();

            var paramNames = new List<string>();

            var columnNames = new List<string>();

            var columnNameWithdataTypes = new List<string>();

            var paramIndex = 0;

            var tableNameAttr = entity.GetType();//.GetCustomAttribute<TableName>(false);

            var tableName = tableNameAttr != null ? tableNameAttr.Name : "";

            int index = 0;

            foreach (var propertyInfo in props)
            {

                var columnNameAttr = propertyInfo.Name;//GetCustomAttribute<ColumnName>(true);

                var columnName = columnNameAttr;// columnNameAttr != null ? columnNameAttr.Name : "";

                // var jsonTypeAttribute = propertyInfo.GetCustomAttribute<JsonType>(false);

                //if (jsonTypeAttribute != null)
                //{
                //    var columnType = jsonTypeAttribute.Type;
                //}

                object value = propertyInfo.GetValue(entity);

                index++;

                var paramName = string.Format("@Param_{0}", columnName);

                columnNames.Add(string.Format(@"{0}", columnName));

                paramNames.Add(paramName);

                string propWithDataType = null;

                var parameter = GetParameter(columnName, propertyInfo.PropertyType, value, index, ref propWithDataType);

                columnNameWithdataTypes.Add(string.Format(@"{0} {1}", columnName, propWithDataType));

                command.Parameters.Add(parameter);
            }

            command.CommandText = String.Format("INSERT INTO " + this.schema + ".{0} ({1}) VALUES({2})", tableName,
                String.Join(",", columnNames.ToArray()), String.Join(",", paramNames.ToArray()));
            Console.WriteLine(command.CommandText);
            //   if (DropandCreateTable == true)
            {
                //var createTableCommand = connection.CreateCommand();

                //createTableCommand.CommandText = string.Format("drop table if exists test2.{0};", tableName);

                //createTableCommand.ExecuteNonQuery();

                //createTableCommand.CommandText = string.Format("Create table test2.{0}({1});", tableName,
                //    string.Join(",", columnNameWithdataTypes));

                //createTableCommand.ExecuteNonQuery();
            }

            return command;
        }
        public object ExecuteScalar(string commandText)
        {
            object result = null;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;
                        result = command.ExecuteScalar();
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {

            }

            return result;
        }
        public void Update(TEntity entity)
        {

        }
        public bool ExecuteSqlQuery(string commandText)
        {
            bool result = false;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

            return result;
        }
    }
}
