using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据查询帮助对象
    /// </summary>
    public partial class PgSqlHelper
    {
        /// <summary>
        ///  数据库命令执行对象
        /// </summary>
        public partial class _execute : PgExecute
        {
            public _execute(ILogger logger, string[] connectionString, int poolSize) : base(logger, connectionString, poolSize) { }
            public _execute(ILogger logger, string connectionString, int poolSize) : base(logger, connectionString, poolSize) { }
        }

        private static _execute instanceMaster = null;
        /// <summary>
        ///  主数据库实例
        /// </summary>
        public static PgExecute InstanceMaster
        {
            get
            {
                return instanceMaster;
            }
        }

        private static _execute instanceSlave = null;
        /// <summary>
        ///  从库数据库实例
        /// </summary>
        public static PgExecute InstanceSlave
        {
            get
            {
                return instanceSlave;
            }
        }

        /// <summary>
        ///  初始化数据库连接
        /// </summary>
        /// <param name="logger">日志组件</param>
        /// <param name="connectionMaster">可读写数据库连接</param>
        /// <param name="connectionStringSlave">从库数据库连接</param>
        public static void InitConnection(ILogger logger, string connectionMaster, string[] connectionSlaves = null)
        {
            if (string.IsNullOrEmpty(connectionMaster))
                throw new ArgumentNullException("connectionString not null");

            // 初始化主库连接实例
            int poolsizeMaster = GetPollSize(connectionMaster);
            instanceMaster = new _execute(logger, connectionMaster, poolsizeMaster);

            // 初始化从库连接实例
            if (connectionSlaves != null && connectionSlaves.Length > 0)
            {
                int pollsizeSlave = GetPollSize(connectionSlaves.First());
                instanceSlave = new _execute(logger, connectionSlaves, pollsizeSlave);
            }
        }

        /// <summary>
        ///  获取数据库连接池配置
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static int GetPollSize(string connectionString)
        {
            int size = 32;
            Match m = Regex.Match(connectionString.ToLower(), @"maximum\s*pool\s*size\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                int.TryParse(m.Groups[1].Value, out size);

            if (size <= 0)
                size = 32;

            return size;
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static object ExecuteScalar(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return InstanceMaster.ExecuteScalar(commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return InstanceMaster.ExecuteNonQuery(commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteDataReader(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            InstanceMaster.ExecuteDataReader(action, commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static object ExecuteScalarSlave(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return InstanceSlave.ExecuteScalar(commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static int ExecuteNonQuerySlave(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            return InstanceSlave.ExecuteNonQuery(commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteDataReaderSlave(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            InstanceSlave.ExecuteDataReader(action, commandType, commandText, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static void Transaction(Action action)
        {
            try
            {
                InstanceMaster.BeginTransaction();
                action?.Invoke();
                InstanceMaster.CommitTransaction();
            }
            catch (Exception e)
            {
                InstanceMaster.RollBackTransaction();
                throw e;
            }
        }
    }
}

