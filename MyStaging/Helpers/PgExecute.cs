using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace MyStaging.Helpers
{
    public abstract class PgExecute
    {
        #region Identity        
        public ConnectionPool Pool { get; set; }
        public ILogger _logger = null;
        private Dictionary<int, NpgsqlTransaction> _trans = new Dictionary<int, NpgsqlTransaction>();
        private object _trans_lock = new object();
        public PgExecute() { }
        public PgExecute(ILogger logger, string connectionString)
        {
            _logger = logger;
            if (_logger == null)
                _logger = new LoggerFactory().CreateLogger<PgExecute>();
            Pool = new ConnectionPool(connectionString);
        }
        #endregion

        private NpgsqlTransaction CurrentThreadTransaction
        {
            get
            {
                int tid = Thread.CurrentThread.ManagedThreadId;
                if (_trans.ContainsKey(tid) && _trans[tid] != null)
                    return _trans[tid];
                return null;
            }
        }

        protected void AttachParameters(NpgsqlCommand command, NpgsqlParameter[] commandParameters)
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

        protected void PrepareCommand(NpgsqlCommand command, CommandType commandType, string commandText, NpgsqlParameter[] commandParameters)
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

        public object ExecuteScalar(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            object retval = null;
            NpgsqlCommand cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(cmd, commandType, commandText, commandParameters);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                retval = cmd.ExecuteScalar();
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

        public int ExecuteNonQuery(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
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

        public void ExecuteDataReader(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
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

        protected void ExceptionOutPut(NpgsqlCommand cmd, Exception ex)
        {
            string ps = string.Empty;
            if (cmd.Parameters != null)
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    var item = cmd.Parameters[i];
                    ps += $"{ item.ParameterName}:{item.Value},";
                }
            if (_logger != null)
                _logger.LogError(new EventId(111111), ex, "数据库执行出错：===== \n {0}\n{1}\n{2}", cmd.CommandText, cmd.Parameters, ps);
        }

        public void BeginTransaction()
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

        public void CommitTransaction()
        {
            CommitTransaction(true);
        }

        public void CommitTransaction(bool iscommit)
        {
            int tid = Thread.CurrentThread.ManagedThreadId;

            NpgsqlTransaction tran = CurrentThreadTransaction;
            if (tran != null)
            {
                if (iscommit)
                    tran.Commit();
                else
                    tran.Rollback();

                tran.Dispose();
                lock (_trans_lock)
                {
                    _trans.Remove(tid);
                }
            }
            this.Pool.FreeConnection(tran?.Connection);
        }

        public void RollBackTransaction()
        {
            NpgsqlTransaction tran = CurrentThreadTransaction;
            if (tran != null && !tran.IsCompleted)
            {
                CommitTransaction(false);
            }
        }
    }
}
