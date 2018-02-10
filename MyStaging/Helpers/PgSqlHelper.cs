using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace MyStaging.Helpers
{
    public partial class PgSqlHelper
    {
        public partial class _execute : PgExecute
        {
            public _execute(ILogger logger, string connectionString, int poolSize) : base(logger, connectionString, poolSize) { }
        }

        private static _execute _instance = null;
        public static PgExecute Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new _execute(_logger, _connectionString, poolsize);

                return _instance;
            }
        }
        private static int poolsize = 0;
        private static string _connectionString = string.Empty;
        private static ILogger _logger;
        public static void InitConnection(ILogger logger, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString not null");

            _logger = logger;
            _connectionString = connectionString;


            Match m = Regex.Match(connectionString.ToLower(), @"maximum\s*pool\s*size\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                int.TryParse(m.Groups[1].Value, out poolsize);
            else
                poolsize = 32;
            if (poolsize <= 0)
                poolsize = 32;


        }

        public static object ExecuteScalar(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return Instance.ExecuteScalar(commandType, commandText, commandParameters);
        }

        public static int ExecuteNonQuery(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return Instance.ExecuteNonQuery(commandType, commandText, commandParameters);
        }

        public static void ExecuteDataReader(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            Instance.ExecuteDataReader(action, commandType, commandText, commandParameters);
        }
        public static void Transaction(Action action)
        {
            try
            {
                Instance.BeginTransaction();
                action?.Invoke();
                Instance.CommitTransaction();
            }
            catch (Exception e)
            {
                Instance.RollBackTransaction();
                throw e;
            }
        }
    }
}

