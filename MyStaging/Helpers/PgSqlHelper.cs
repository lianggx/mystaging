using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.WebSockets;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据查询帮助对象
    /// </summary>
    public partial class PgSqlHelper
    {
        /// <summary>
        /// 提供外部订阅的异常接口
        /// </summary>
        public static Action<object, Exception> OnException;

        /// <summary>
        ///  数据库命令执行对象
        /// </summary>
        public partial class MasterExecute : PgExecute
        {
            public MasterExecute(ILogger logger, string connectionString, int poolSize) : base(logger, connectionString, poolSize) { }
        }

        /// <summary>
        ///  数据库命令执行对象
        /// </summary>
        public partial class SlaveExecute : PgExecute
        {
            public SlaveExecute(ILogger logger, string[] connectionString, int poolSize) : base(logger, connectionString, poolSize) { }
        }

        private static MasterExecute instanceMaster = null;
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

        private static SlaveExecute instanceSlave = null;
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
            instanceMaster = new MasterExecute(logger, connectionMaster, poolsizeMaster);

            // 初始化从库连接实例
            if (connectionSlaves != null && connectionSlaves.Length > 0)
            {
                int pollsizeSlave = GetPollSize(connectionSlaves.First());
                instanceSlave = new SlaveExecute(logger, connectionSlaves, pollsizeSlave);
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
            object result = null;
            void Transfer(Exception ex)
            {
                RemoveConnection(InstanceSlave, ex);
                if (instanceSlave != null && instanceSlave.Pool.ConnectionList.Count > 0)
                {
                    result = ExecuteScalarSlave(commandType, commandText, commandParameters);
                }
                else
                {
                    WriteLog("The database all connection refused，transfer to database master");
                    result = instanceMaster.ExecuteScalar(commandType, commandText, commandParameters);
                }
            }

            try
            {
                if (instanceSlave != null && instanceSlave.Pool.ConnectionList.Count > 0)
                    result = InstanceSlave.ExecuteScalar(commandType, commandText, commandParameters);
                else
                {
                    WriteLog("The database slave connection zero，transfer to database master");
                    result = instanceMaster.ExecuteScalar(commandType, commandText, commandParameters);
                }
            }
            catch (System.TimeoutException te)
            {
                Transfer(te);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Transfer(ex);
            }
            return result;
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteDataReaderSlave(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            void Transfer(Exception ex)
            {
                RemoveConnection(instanceSlave, ex);
                if (instanceSlave != null && instanceSlave.Pool.ConnectionList.Count > 0)
                {
                    ExecuteDataReaderSlave(action, commandType, commandText, commandParameters);
                }
                else
                {
                    WriteLog("The database all connection refused，transfer to database master");
                    instanceMaster.ExecuteDataReader(action, commandType, commandText, commandParameters);
                }
            }

            try
            {
                if (instanceSlave != null && instanceSlave.Pool.ConnectionList.Count > 0)
                    InstanceSlave.ExecuteDataReader(action, commandType, commandText, commandParameters);
                else
                {
                    WriteLog("The database slave connection zero，transfer to database master");
                    instanceMaster.ExecuteDataReader(action, commandType, commandText, commandParameters);
                }
            }
            catch (System.TimeoutException te)
            {
                Transfer(te);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Transfer(ex);
            }
        }

        /// <summary>
        ///  移除从库连接，记录日志
        /// </summary>
        /// <param name="ex"></param>
        private static void RemoveConnection(object sender, Exception ex)
        {
            var host = ex.Data["host"].ToString();
            var port = Convert.ToInt32(ex.Data["port"]);
            string message = string.Format("The database slave[{0}:{1}] connection refused，transfer slave the others.{2}", host, port, ex.StackTrace);
            WriteLog(message);
            instanceSlave.Pool.RemoveConnection(host, port);
            // 传递异常
            OnException?.Invoke(sender, ex);
        }

        /// <summary>
        ///  记录连接异常日志
        /// </summary>
        /// <param name="message"></param>
        private static void WriteLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            if (instanceMaster._logger != null)
                instanceMaster._logger.LogError(message);
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

