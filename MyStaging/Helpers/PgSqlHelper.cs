using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MyStaging.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using MyStaging.Mapping;
using MyStaging.Interface;

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
            /// <summary>
            /// 
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="connectionString"></param>
            public MasterExecute(ILogger logger, ConnectionStringConfiguration connectionString) : base(logger, connectionString) { }
        }

        /// <summary>
        ///  数据库命令执行对象
        /// </summary>
        public partial class SlaveExecute : PgExecute
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="connectionString"></param>
            public SlaveExecute(ILogger logger, List<ConnectionStringConfiguration> connectionString) : base(logger, connectionString) { }
        }

        /// <summary>
        ///  主数据库实例
        /// </summary>
        public static PgExecute InstanceMaster { get; set; }

        /// <summary>
        ///  从库数据库实例
        /// </summary>
        public static PgExecute InstanceSlave { get; set; }

        /// <summary>
        ///  缓存管理
        /// </summary>
        public static CacheManager CacheManager { get; set; } = null;

        /// <summary>
        ///  脚手架设置选项
        /// </summary>
        public static StagingOptions Options { get; set; }

        /// <summary>
        ///  初始化数据库连接
        /// </summary>
        /// <param name="options"></param>
        public static void InitConnection(StagingOptions options)
        {
            CheckNotNull.NotNull(options, nameof(options));
            CheckNotNull.NotEmpty(options.ConnectionMaster, nameof(options.ConnectionMaster));

            Options = options;

            // 初始化主库连接实例
            ConnectionStringConfiguration conn = new ConnectionStringConfiguration()
            {
                ConnectionString = options.ConnectionMaster,
                DbConnection = new Npgsql.NpgsqlConnection(options.ConnectionMaster)
            };
            InstanceMaster = new MasterExecute(options.Logger, conn);

            // 初始化从库连接实例
            List<ConnectionStringConfiguration> connList = GetSlaves(options.ConnectionSlaves);

            if (connList != null)
                InstanceSlave = new SlaveExecute(options.Logger, connList);

            if (options.CacheOptions != null && options.CacheOptions.Cache != null)
            {
                CacheManager = new CacheManager(options.CacheOptions);
            }
        }

        private static List<ConnectionStringConfiguration> GetSlaves(string[] connectionSlaves)
        {
            List<ConnectionStringConfiguration> connList = null;
            if (connectionSlaves != null && connectionSlaves.Length > 0)
            {
                connList = new List<ConnectionStringConfiguration>();
                for (int i = 0; i < connectionSlaves.Length; i++)
                {
                    var item = connectionSlaves[i];
                    connList.Add(new ConnectionStringConfiguration()
                    {
                        ConnectionString = item,
                        Id = i,
                        DbConnection = new Npgsql.NpgsqlConnection(item)
                    });
                }
            }

            return connList;
        }

        /// <summary>
        ///  刷新数据库连接
        /// </summary>
        /// <param name="connectionMaster"></param>
        /// <param name="connectionSlaves"></param>
        public static void Refresh(string connectionMaster, string[] connectionSlaves = null)
        {
            ConnectionStringConfiguration conn = new ConnectionStringConfiguration()
            {
                ConnectionString = connectionMaster,
                DbConnection = new Npgsql.NpgsqlConnection(connectionMaster)
            };

            InstanceMaster.Pool.Refresh(new List<ConnectionStringConfiguration>() { conn });
            List<ConnectionStringConfiguration> connList = GetSlaves(connectionSlaves);
            InstanceSlave?.Pool?.Refresh(connList);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return InstanceMaster.ExecuteScalar(commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            return InstanceMaster.ExecuteScalar(commandType, commandText, onExecuted, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return ExecuteNonQuery(commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            return InstanceMaster.ExecuteNonQuery(commandType, commandText, onExecuted, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        public static void ExecuteDataReader(Action<DbDataReader> action, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            ExecuteDataReader(action, commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在读写数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        public static void ExecuteDataReader(Action<DbDataReader> action, CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            InstanceMaster.ExecuteDataReader(action, commandType, commandText, onExecuted, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static object ExecuteScalarSlave(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return ExecuteScalarSlave(commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static object ExecuteScalarSlave(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            object result = null;
            void Transfer(Exception ex)
            {
                if (InstanceSlave != null && InstanceSlave.Pool.ConnectionList.Count > 0)
                {
                    result = InstanceSlave.ExecuteScalar(commandType, commandText, onExecuted, commandParameters);
                }
                else
                {
                    result = InstanceMaster.ExecuteScalar(commandType, commandText, onExecuted, commandParameters);
                }
            }

            try
            {
                Transfer(null);
            }
            catch (System.TimeoutException te)
            {
                WriteLog(te);
                Transfer(te);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                WriteLog(ex);
                Transfer(ex);
            }
            return result;
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        public static void ExecuteDataReaderSlave(Action<DbDataReader> action, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            ExecuteDataReaderSlave(action, commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        public static void ExecuteDataReaderSlave(Action<DbDataReader> action, CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            void Transfer(Exception ex)
            {
                if (InstanceSlave != null && InstanceSlave.Pool.ConnectionList.Count > 0)
                {
                    InstanceSlave.ExecuteDataReader(action, commandType, commandText, onExecuted, commandParameters);
                }
                else
                {
                    InstanceMaster.ExecuteDataReader(action, commandType, commandText, onExecuted, commandParameters);
                }
            }

            try
            {
                Transfer(null);
            }
            catch (System.TimeoutException te)
            {
                WriteLog(te);
                Transfer(te);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                WriteLog(ex);
                Transfer(ex);
            }
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuerySlave(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return ExecuteNonQuery(commandType, commandText, null, commandParameters);
        }

        /// <summary>
        ///  此函数只能在从库数据库连接中进行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="onExecuted"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuerySlave(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            return InstanceSlave.ExecuteNonQuery(commandType, commandText, onExecuted, commandParameters);
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
            Options.Logger?.LogError(message);
        }

        private static void WriteLog(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            var dbConnection = ex.Data["DbConnection"] as DbConnection;
            if (dbConnection != null)
            {
                sb.AppendLine(dbConnection.ConnectionString);
            }

            if (ex.InnerException != null)
            {
                sb.AppendLine(ex.InnerException.Message);
                sb.AppendLine(ex.InnerException.StackTrace);
            }

            WriteLog(sb.ToString());
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
            catch (Exception ex)
            {
                WriteLog(ex);
                InstanceMaster.RollBackTransaction();
                throw;
            }
        }

        /// <summary>
        ///  封装多个查询结果集，以管道的形式
        /// </summary>
        /// <param name="master">是否在主库执行查询</param>
        /// <param name="contexts">查询上下文对象</param>
        /// <returns></returns>
        public static List<List<dynamic>> ExecutePipeLine(bool master, params IQueryPipe[] contexts)
        {
            CheckNotNull.NotEmpty(contexts, nameof(contexts));

            StringBuilder sb = new StringBuilder();
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (var ctx in contexts)
            {
                sb.AppendLine(ctx.CommandText);
                sb.Append(";");
                parameters.AddRange(ctx.ParamList);
            }

            var cmdText = sb.ToString();
            int pipeLine = contexts.Length;
            List<List<dynamic>> result = new List<List<dynamic>>();
            if (master)
            {
                CheckNotNull.NotNull(InstanceMaster, nameof(InstanceMaster));

                InstanceMaster.ExecuteDataReaderPipe(dr =>
                {
                    ExcutePipeResult(contexts, dr, pipeLine, result);

                }, CommandType.Text, cmdText, parameters.ToArray());
            }
            else
            {
                CheckNotNull.NotNull(InstanceSlave, nameof(InstanceSlave));

                InstanceSlave.ExecuteDataReaderPipe(dr =>
                {
                    ExcutePipeResult(contexts, dr, pipeLine, result);

                }, CommandType.Text, cmdText, parameters.ToArray());
            }

            return result;
        }

        private static void ExcutePipeResult(IQueryPipe[] contexts, DbDataReader dr, int pipeLine, List<List<dynamic>> result)
        {
            for (int i = 0; i < pipeLine; i++)
            {
                List<dynamic> list = new List<dynamic>();
                var ctx = contexts[i];
                var builder = new DynamicBuilder(ctx.PipeResultType).CreateBuilder(dr);
                while (dr.Read())
                {
                    var obj = ctx.ReadObj(builder, dr, ctx.PipeResultType);
                    list.Add(obj);
                };
                dr.NextResult();
                result.Add(list);
            }
        }
    }
}

