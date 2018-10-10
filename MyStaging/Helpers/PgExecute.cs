using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
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

        private Dictionary<int, NpgsqlTransaction> _trans = new Dictionary<int, NpgsqlTransaction>();
        private object _trans_lock = new object();

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
        public PgExecute(ILogger logger, string connectionMaster, int poolSize) : this(logger, new string[] { connectionMaster }, poolSize)
        {
        }

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="logger">日志输出对象</param>
        /// <param name="connectionSalve">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public PgExecute(ILogger logger, string[] connectionSalve, int poolSize)
        {
            _logger = logger;
            if (_logger == null)
                _logger = new LoggerFactory().CreateLogger<PgExecute>();
            Pool = new ConnectionPool(connectionSalve, poolSize);
        }
        #endregion

        /// <summary>
        ///  获取当前线程产生的数据库事务
        /// </summary>
        protected virtual NpgsqlTransaction CurrentThreadTransaction
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
        protected virtual void AttachParameters(NpgsqlCommand command, NpgsqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters == null) return;

            foreach (NpgsqlParameter p in commandParameters)
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
        protected virtual void PrepareCommand(NpgsqlCommand command, CommandType commandType, string commandText, NpgsqlParameter[] commandParameters)
        {
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");
            if (CurrentThreadTransaction != null)
            {
                command.Connection = CurrentThreadTransaction.Connection;
                command.Transaction = CurrentThreadTransaction;
            }
            else
            {
                command.Connection = this.Pool.GetConnection();
            }

            command.CommandText = commandText;
            command.CommandType = commandType;

            if (commandParameters != null)
                AttachParameters(command, commandParameters);
        }

        /// <summary>
        ///  执行查询，并返回第一行数据的第一列的值
        /// </summary>
        /// <param name="commandType">CommandType 类型</param>
        /// <param name="commandText">待执行的 SQL 语句</param>
        /// <param name="commandParameters">NpgsqlCommand 对象的参数列表</param>
        /// <returns></returns>
        public virtual object ExecuteScalar(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            bool connected = true;
            object retval = null;
            NpgsqlCommand cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(cmd, commandType, commandText, commandParameters);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                retval = cmd.ExecuteScalar();
            }
            catch (SocketException se)
            {
                connected = false;
                se.Data["host"] = cmd.Connection.Host;
                se.Data["port"] = cmd.Connection.Port;
                ExceptionOutPut(cmd, se);
                throw se;
            }
            catch (Exception ex)
            {
                ex.Data["host"] = cmd.Connection.Host;
                ex.Data["port"] = cmd.Connection.Port;
                ExceptionOutPut(cmd, ex);
                throw ex;
            }
            finally
            {
                if (connected && this.CurrentThreadTransaction == null)
                {
                    this.Pool.FreeConnection(cmd.Connection);
                }

                cmd.Parameters.Clear();
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
        public virtual int ExecuteNonQuery(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            int retval = 0;
            NpgsqlCommand cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(cmd, commandType, commandText, commandParameters);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                retval = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ExceptionOutPut(cmd, ex);
                throw ex;
            }
            finally
            {
                if (this.CurrentThreadTransaction == null)
                {
                    this.Pool.FreeConnection(cmd.Connection);
                }
                cmd.Parameters.Clear();
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
        public virtual void ExecuteDataReader(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            NpgsqlDataReader reader = null;
            try
            {
                PrepareCommand(cmd, commandType, commandText, commandParameters);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    action?.Invoke(reader);
                };
            }
            catch (Exception ex)
            {
                ExceptionOutPut(cmd, ex);
                throw ex;
            }
            finally
            {
                if (this.CurrentThreadTransaction == null)
                {
                    this.Pool.FreeConnection(cmd.Connection);
                }
                if (reader != null)
                    reader.Close();
                cmd.Parameters.Clear();
            }
        }

        /// <summary>
        ///  输出异常信息
        /// </summary>
        /// <param name="cmd">NpgsqlCommand 对象</param>
        /// <param name="ex">异常信息</param>
        protected virtual void ExceptionOutPut(NpgsqlCommand cmd, Exception ex)
        {
            NpgsqlParameterCollection coll = cmd.Parameters;

            string ps = string.Empty;
            if (coll != null)
                for (int i = 0; i < coll.Count; i++)
                {
                    var item = coll[i];
                    ps += $"{ item.ParameterName}:{item.Value},";
                }
            string sql = cmd.CommandText;
            for (int i = 0; i < coll.Count; i++)
            {
                var para = coll[i];
                var isString = IsString(para.NpgsqlDbType);
                var val = string.Format("{0}{1}{0}", isString ? "'" : "", para.Value.ToString());
                sql = sql.Replace("@" + para.ParameterName, val);
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
        private bool IsString(NpgsqlDbType dbType)
        {
            switch (dbType)
            {
                case NpgsqlDbType.Integer:
                case NpgsqlDbType.Numeric:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        ///  在当前线程上开始执行事务
        /// </summary>
        public virtual void BeginTransaction()
        {
            if (CurrentThreadTransaction != null)
                CommitTransaction(true);

            NpgsqlConnection Connection = this.Pool.GetConnection();
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            NpgsqlTransaction tran = Connection.BeginTransaction();
            int tid = Thread.CurrentThread.ManagedThreadId;
            if (_trans.ContainsKey(tid))
                CommitTransaction();

            lock (_trans_lock)
            {
                _trans.Add(tid, tran);
            }
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
        public virtual void CommitTransaction(bool iscommit)
        {
            NpgsqlTransaction tran = CurrentThreadTransaction;
            if (tran == null || tran.Connection == null) return;

            lock (_trans_lock)
            {
                int tid = Thread.CurrentThread.ManagedThreadId;
                _trans.Remove(tid);
            }
            NpgsqlConnection conn = tran.Connection;
            if (iscommit)
                tran.Commit();
            else
                tran.Rollback();

            this.Pool.FreeConnection(conn);
        }

        /// <summary>
        ///  将当前线程上的事务进行回滚
        /// </summary>
        public virtual void RollBackTransaction()
        {
            NpgsqlTransaction tran = CurrentThreadTransaction;
            if (tran != null && !tran.IsCompleted)
            {
                CommitTransaction(false);
            }
        }
    }
}
