using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace DataAnalyticsPlatform.Shared.DataAccess
{
    public interface IBaseModel
    {
        string ModelName { get; set; }

        string[] Props { get; set; }

        object[] Values { get; set; }
    }

    public class BaseModel : IBaseModel//abstract
    {
        public string ModelName { get; set; }
        public long RecordId1 { get; set; }
        public long FileId1 { get; set; }
        public string[] Props { get; set; }
        public object[] Values { get; set; }
    }

    public class BulkPostgresRepository<TEntity> where TEntity : IBaseModel
    {


        private string _schemaName;

        private string _connectionString;

        private Dictionary<string, ImporterInfo> _importersDictionaryByTableName;

        public BulkPostgresRepository(string connectionString, string schemaName)
        {
            _connectionString = connectionString;

            _importersDictionaryByTableName = new Dictionary<string, ImporterInfo>();

            _schemaName = schemaName;


        }

        public void CreateSchema()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("create schema if not exists " + this._schemaName);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Write(List<TEntity> entities)
        {
            ImporterInfo importerInfo = null;

            if (_importersDictionaryByTableName.ContainsKey(entities[0].ModelName) == false)
            {
                importerInfo = new ImporterInfo();

                importerInfo.TableName = entities[0].ModelName;

                importerInfo.Columns = GetColumns(entities[0]);

                CreateTable(importerInfo.GetCreateTableStatement(_schemaName));

                _importersDictionaryByTableName.Add(importerInfo.TableName, importerInfo);
            }
            else
            {
                importerInfo = _importersDictionaryByTableName[entities[0].ModelName];
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var writeStatement = importerInfo.GetWriteStatement(_schemaName);

                using (var writer = connection.BeginBinaryImport(writeStatement))
                {
                    Dictionary<string, NpgsqlDbType> colsDbType = new Dictionary<string, NpgsqlDbType>();


                    foreach (var item in entities)
                    {
                        writer.StartRow();

                        for (int i = 0; i < item.Values.Length; i++)
                        {
                            NpgsqlDbType dbType = NpgsqlDbType.Unknown;
                            string propName = item.Props[i];
                            if (colsDbType.TryGetValue(propName, out dbType) == false)
                            {
                                var col  = importerInfo.Columns.Find(x => x.Item1 == propName);

                                dbType = col.Item3;

                                colsDbType.Add(propName, dbType);
                            }

                            if (item.Values[i] is null)
                            {
                                writer.WriteNull();
                            }
                            else
                            {
                                writer.Write(item.Values[i], dbType);
                            }
                        }
                    }

                    writer.Complete();
                }
            }


        }

        private void CreateTable(string v)
        {
            string functions = "";
            using (StreamReader sr = new StreamReader("PostgresFunctions.txt"))
            {
                functions = sr.ReadToEnd();
                functions = functions.Replace("schemaholder", _schemaName);
            }
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = v;

                    command.ExecuteNonQuery();

                    command.CommandText = functions;

                    command.ExecuteNonQuery();
                }
            }
            
        }

        private List<(string, string, NpgsqlDbType)> GetColumns(TEntity entity)
        {

            List<(string, string, NpgsqlDbType)> result = new List<(string, string, NpgsqlDbType)>();

            foreach (var propName in entity.Props)
            {
                var propertyInfo = entity.GetType().GetProperty(propName);

                string columnName = propertyInfo.Name;

                string pgDbType = string.Empty;

                var propType = propertyInfo.PropertyType;

                NpgsqlDbType npgsqlDBType = NpgsqlDbType.Unknown;
                if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    Type t = Nullable.GetUnderlyingType(propType);
                    propType = t;
                }

                if (propType == typeof(SByte))
                {
                    npgsqlDBType = NpgsqlDbType.Smallint;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Byte))
                {
                    npgsqlDBType = NpgsqlDbType.Smallint;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Int64))
                {
                    npgsqlDBType = NpgsqlDbType.Bigint;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Int32))
                {
                    npgsqlDBType = NpgsqlDbType.Integer;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Int16))
                {
                    npgsqlDBType = NpgsqlDbType.Smallint;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(UInt16))
                {
                    npgsqlDBType = NpgsqlDbType.Smallint;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(UInt32))
                {
                    npgsqlDBType = NpgsqlDbType.Integer;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(double))
                {
                    npgsqlDBType = NpgsqlDbType.Real;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Double))
                {
                    npgsqlDBType = NpgsqlDbType.Real;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Single))
                {
                    npgsqlDBType = NpgsqlDbType.Real;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(string))
                {

                    npgsqlDBType = NpgsqlDbType.Varchar;
                    pgDbType = npgsqlDBType.ToString();

                }
                else if (propType == typeof(DateTime))
                {

                    npgsqlDBType = NpgsqlDbType.Timestamp;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(System.Decimal))
                {
                    npgsqlDBType = NpgsqlDbType.Numeric;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(byte[]))
                {
                    npgsqlDBType = NpgsqlDbType.Bytea;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Boolean))
                {
                    npgsqlDBType = NpgsqlDbType.Boolean;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(String[]) || propType == typeof(string[]))
                {

                    npgsqlDBType = NpgsqlDbType.Text;
                    pgDbType = npgsqlDBType.ToString();

                }
                else if (propType == typeof(Single[]))
                {
                    npgsqlDBType = NpgsqlDbType.Text;
                    pgDbType = npgsqlDBType.ToString();
                }
                else if (propType == typeof(Int32[]))
                {
                    npgsqlDBType = NpgsqlDbType.Array;
                    pgDbType = npgsqlDBType.ToString();
                }
                else
                {
                    if (propType.GetTypeInfo().GetType() != typeof(int))
                    {
                        npgsqlDBType = NpgsqlDbType.Text;
                        pgDbType = npgsqlDBType.ToString();
                    }
                }

                result.Add((columnName, pgDbType, npgsqlDBType));
            }

            return result;
        }
    }

    public class ImporterInfo
    {
        public NpgsqlBinaryImporter Importer { get; set; }

        public string TableName { get; set; }

        //tablename, pg data type, .net data type
        public List<ValueTuple<string, string,NpgsqlDbType>> Columns { get; set; }

        public ImporterInfo()
        {
            Columns = new List<(string, string, NpgsqlDbType)>();
        }
    }
}
