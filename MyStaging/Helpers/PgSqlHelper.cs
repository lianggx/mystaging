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
            public _execute() { }
        }
        static PgExecute _instance = null;
        private static PgExecute Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new _execute();

                return _instance;
            }
        }
        private static ILogger _logger;
        public static void InitConnection(ILogger logger, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString not null");

            int poolsize = 0;
            Match m = Regex.Match(connectionString.ToLower(), @"maximum\s*pool\s*size\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                int.TryParse(m.Groups[1].Value, out poolsize);
            else
                poolsize = 32;
            if (poolsize <= 0)
                poolsize = 32;

            _logger = logger;
            PgExecute._logger = _logger;
            ConnectionPool.Connection_String = connectionString;
            ConnectionPool.Pool_Size = poolsize;
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

