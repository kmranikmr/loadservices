using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace LoadServiceApi.Shared
{
    public partial class PgRepository
    {
        private NpgsqlConnection _connection;

        private readonly Dictionary<string, NpgsqlCommand> _eventNameToDbCommand;

        private NpgsqlTransaction _transaction;

        private int _counter;

        private int _batchSize = 100000;

        private bool isConnected = false;

        private string _connectionString;


        public PgRepository(string connectionString, string schemaName = "test")
        {
            _connectionString = connectionString;
            _eventNameToDbCommand = new Dictionary<string, NpgsqlCommand>();


        }

        public PgRepository()
        {
            _eventNameToDbCommand = new Dictionary<string, NpgsqlCommand>();
        }

        public void Connect(string connectionString)
        {
            // if (!isConnected)
            {
                _connectionString = connectionString;
                _connection = new NpgsqlConnection(connectionString);
                _connection.Open();
                isConnected = true;
            }
            // if (_postgresMapper == null)
            //    _postgresMapper = new PostgresMapper();
            //   _mapper = GetTets();
        }


        public List<Tuple<string, string>> GetTableInfo(string sql)
        {
            var tableinfo = new List<Tuple<string, string>>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tableinfo.Add(new Tuple<string, string>(reader[0].ToString(), reader[1].ToString()));
                    }
                }
            }
            return tableinfo;
        }

        public object ExecuteScalar(string commandText)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    return command.ExecuteScalar();
                }
            }
        }

        public DataTable GetResultTable(string commandText)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(commandText, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {


            }

            return dt;

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

            return result;
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



        public void Vectorize(string jsonData)
        {

        }





        //suport for Dapper
        public IEnumerable<T> GetRows<T>(string query, string connectionString)
        {

            using (_connection = new NpgsqlConnection(connectionString))
            {
                return _connection.Query<T>(query);
            }

        }

        public void Dispose()
        {
            if (_counter > 0 && _transaction != null)
            {
                Commit();
            }

            foreach (var npgsqlCommand in _eventNameToDbCommand)
            {
                // npgsqlCommand.Value.Dispose();
            }

            //  _eventNameToDbCommand.Clear();

            if (_connection != null)
            {
                _connection.Close();

                _connection.Dispose();
            }

            //Console.WriteLine("Postgres connection disposed.");
        }
    }

}



