using Microsoft.Extensions.Logging;
using MyStaging.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库语句执行对象，抽象类
    /// </summary>
    public abstract class PgExecute
    {
        #region Identity
        /// <summary>
        ///  获取或者设置数据库连接池对象
        /// </summary>
        public virtual ConnectionPool Pool { get; set; }

        /// <summary>
        ///  日志输出对象
        /// </summary>
        public ILogger _logger = null;

        private ConcurrentDictionary<int, DbTransaction> _trans = new ConcurrentDictionary<int, DbTransaction>();

        /// <summary>
        ///  默认构造函数
        /// </summary>
        public PgExecute() { }

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="logger">日志输出对象</param>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public PgExecute(ILogger logger, ConnectionStringConfiguration connectionString) : this(logger, new List<ConnectionStringConfiguration>() { connectionString })
        {
        }

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="logger">日志输出对象</param>
        /// <param name="connectionSalve">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public PgExecute(ILogger logger, List<ConnectionStringConfiguration> connectionList) : this(logger, new ConnectionPool(connectionList))
        {

        }

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="logger">日志输出对象</param>
        /// <param name="connectionSalve">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public PgExecute(ILogger logger, ConnectionPool pool)
        {
            _logger = logger;
            if (_logger == null)
                _logger = new LoggerFactory().CreateLogger<PgExecute>();
            Pool = pool;
        }
        #endregion

        /// <summary>
        ///  获取当前线程产生的数据库事务
        /// </summary>
        protected virtual DbTransaction CurrentThreadTransaction
        {
            get
            {
                int tid = Thread.CurrentThread.ManagedThreadId;
                if (_trans.ContainsKey(tid) && _trans[tid] != null)
                    return _trans[tid];
                return null;
            }
        }

        /// <summary>
        ///  将 NpgsqlParameter 附加到待执行的 NpgsqlCommand 中
        /// </summary>
        /// <param name="command">NpgsqlCommand 对象</param>
        /// <param name="commandParameters">NpgsqlParameter 数组</param>
        protected virtual void AttachParameters(DbCommand command, DbParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters == null) return;

            foreach (DbParameter p in commandParameters)
            {
                if (p == null) continue;
                if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) && p.Value == null)
                    p.Value = DBNull.Value;

                command.Parameters.Add(p);
            }

        }

        /// <summary>
        ///  构造 NpgsqlCommand 对象，初始化连接操作
        /// </summary>
        /// <param name="command">NpgsqlCommand 对象</param>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        protected virtual DbCommand PrepareCommand(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            DbCommand command;
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");
            if (CurrentThreadTransaction != null)
            {
                command = CurrentThreadTransaction.Connection.CreateCommand();
                command.Transaction = CurrentThreadTransaction;
            }
            else
            {
                command = this.Pool.GetConnection().CreateCommand();
            }

            command.CommandText = commandText;
            command.CommandType = commandType;

            if (commandParameters != null)
                AttachParameters(command, commandParameters);

            return command;
        }

        /// <summary>
        ///  执行查询，并返回第一行数据的第一列的值
        /// </summary>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        /// <returns></returns>
        public virtual object ExecuteScalar(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            object retval = null;
            DbCommand command = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);

                retval = command.ExecuteScalar();
                if (retval is DBNull) return null;
            }
            catch (SocketException se)
            {
                ExceptionOutPut(command, se);
                throw se;
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw ex;
            }
            finally
            {
                Clear(command);
                onExecuted?.Invoke(command);
            }

            return retval;
        }

        /// <summary>
        ///  执行查询，并返回第一行数据的第一列的值
        /// </summary>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        /// <returns></returns>
        public async virtual Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            object retval = null;
            DbCommand command = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);

                retval = await command.ExecuteScalarAsync();
            }
            catch (SocketException se)
            {
                ExceptionOutPut(command, se);
                throw se;
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw ex;
            }
            finally
            {
                Clear(command);
                onExecuted?.Invoke(command);
            }
            return retval;
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            int retval = 0;
            DbCommand command = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);
                retval = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw ex;
            }
            finally
            {
                Clear(command);
                onExecuted?.Invoke(command);
            }

            return retval;
        }

        /// <summary>
        ///  执行查询，并返回受影响的行数
        /// </summary>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        /// <returns></returns>
        public async virtual Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            int retval = 0;
            DbCommand command = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);
                retval = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw ex;
            }
            finally
            {
                Clear(command);
                onExecuted?.Invoke(command);
            }
            return retval;
        }

        /// <summary>
        ///  执行查询，并从返回的流中读取数据，传入委托中
        /// </summary>
        /// <param name="action">处理数据的委托函数</param>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        public virtual void ExecuteDataReader(Action<DbDataReader> action, CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    action?.Invoke(reader);
                };
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw ex;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                Clear(command);
                onExecuted?.Invoke(command);
            }
        }

        /// <summary>
        ///  执行查询，并从返回的流中读取数据，传入委托中
        /// </summary>
        /// <param name="action">处理数据的委托函数</param>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        public async virtual Task ExecuteDataReaderAsync(Action<DbDataReader> action, CommandType commandType, string commandText, Action<DbCommand> onExecuted = null, params DbParameter[] commandParameters)
        {
            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                command = PrepareCommand(commandType, commandText, commandParameters);
                OpenConnection(command);

                reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    action?.Invoke(reader);
                };
            }
            catch (Exception ex)
            {
                ExceptionOutPut(command, ex);
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                Clear(command);
                onExecuted?.Invoke(command);
            }
        }

        /// <summary>
        ///  输出异常信息
        /// </summary>
        /// <param name="command">NpgsqlCommand 对象</param>
        /// <param name="ex">异常信息</param>
        protected virtual void ExceptionOutPut(DbCommand command, Exception ex)
        {
            if (command == null)
                return;

            DbParameterCollection coll = command.Parameters;
            ex.Data["DbConnection"] = command.Connection;
            string ps = string.Empty;
            string sql = command.CommandText;
            if (coll != null)
            {
                for (int i = 0; i < coll.Count; i++)
                {
                    var item = coll[i];
                    ps += $"{ item.ParameterName}:{item.Value},";
                }

                for (int i = 0; i < coll.Count; i++)
                {
                    var para = coll[i];
                    var isString = IsString(para.DbType);
                    var val = string.Format("{0}{1}{0}", isString ? "'" : "", para.Value.ToString());
                    sql = sql.Replace("@" + para.ParameterName, val);
                }
            }
            if (_logger != null)
                _logger.LogError(new EventId(111111), ex, "数据库执行出错：===== \n {0}\n{1}\n{2}", sql, coll, ps);
            else
                Console.WriteLine("数据库执行出错：===== \n {0}\n{1}\n{2}", sql, coll, ps);

        }

        /// <summary>
        ///  判断值是否需要加单引号
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        private bool IsString(DbType dbType)
        {

            switch (dbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Boolean:
                case DbType.VarNumeric:
                case DbType.Currency:
                case DbType.Byte:
                case DbType.Single:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        ///  在当前线程上开始执行事务
        /// </summary>
        public virtual DbConnection BeginTransaction()
        {
            if (CurrentThreadTransaction != null)
                CommitTransaction(true);

            DbConnection Connection = this.Pool.GetConnection();
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            DbTransaction tran = Connection.BeginTransaction();
            int tid = Thread.CurrentThread.ManagedThreadId;
            if (_trans.ContainsKey(tid))
                CommitTransaction();
            else
                _trans.TryAdd(tid, tran);

            return Connection;
        }

        /// <summary>
        ///  提交当前线程上执行的事务
        /// </summary>
        public virtual void CommitTransaction()
        {
            CommitTransaction(true);
        }

        /// <summary>
        ///  可控制的事务提交
        /// </summary>
        /// <param name="iscommit">true=提交事务，false=回滚事务</param>
        public virtual DbConnection CommitTransaction(bool iscommit)
        {
            DbTransaction tran = CurrentThreadTransaction;
            if (tran == null || tran.Connection == null) return null;

            int tid = Thread.CurrentThread.ManagedThreadId;
            _trans.TryRemove(tid, out tran);

            DbConnection connection = tran.Connection;
            if (iscommit)
                tran.Commit();
            else
                tran.Rollback();

            return connection;
        }

        /// <summary>
        ///  将当前线程上的事务进行回滚
        /// </summary>
        public virtual DbConnection RollBackTransaction()
        {
            return CommitTransaction(false);
        }

        /// <summary>
        ///  打开数据库连接
        /// </summary>
        /// <param name="cmd"></param>
        private void OpenConnection(DbCommand cmd)
        {
            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();
        }

        /// <summary>
        ///  释放连接，清理资源
        /// </summary>
        /// <param name="cmd"></param>
        private void Clear(DbCommand cmd)
        {
            if (cmd != null)
            {
                if (this.CurrentThreadTransaction == null)
                {
                    this.Pool.FreeConnection(cmd.Connection);
                }
                cmd.Parameters?.Clear();
            }
        }
    }
}
