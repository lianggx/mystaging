using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Collections;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace MyStaging.Helpers
{
    public abstract class PgExecute
    {
        #region Identity        
        public static ILogger _logger = null;
        public NpgsqlConnection Connection = null;
        public PgExecute(ILogger logger)
        {
            _logger = logger;
        }
        public PgExecute() { }
        #endregion

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
            if (Connection == null)
                Connection = ConnectionPool.GetConnection();
            command.Connection = Connection;
            command.CommandText = commandText;
            command.CommandType = commandType;
            if (commandParameters != null)
                AttachParameters(command, commandParameters);
        }

        public object ExecuteScalar(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            object retval = null;
            NpgsqlCommand _cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(_cmd, commandType, commandText, commandParameters);
                if (_cmd.Connection.State != ConnectionState.Open)
                    _cmd.Connection.Open();
                retval = _cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                ExceptionOutPut(_cmd, ex);
                throw ex;
            }
            finally
            {
                if (_tran == null)
                    Clear(_cmd, _cmd.Connection);
            }
            return retval;
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            int retval = 0;
            NpgsqlCommand _cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(_cmd, commandType, commandText, commandParameters);
                if (_cmd.Connection.State != ConnectionState.Open)
                    _cmd.Connection.Open();
                retval = _cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ExceptionOutPut(_cmd, ex);
                throw ex;
            }
            finally
            {
                if (_tran == null)
                    Clear(_cmd, _cmd.Connection);
            }
            return retval;
        }
        public void ExecuteDataReader(Action<NpgsqlDataReader> action, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            NpgsqlCommand _cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(_cmd, commandType, commandText, commandParameters);
                if (_cmd.Connection.State != ConnectionState.Open)
                    _cmd.Connection.Open();
                using (NpgsqlDataReader reader = _cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        action?.Invoke(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionOutPut(_cmd, ex);
                throw ex;
            }
            finally
            {
                if (_tran == null)
                    Clear(_cmd, _cmd.Connection);
            }
        }
        public void Clear(NpgsqlCommand cmd, NpgsqlConnection conn)
        {
            if (cmd != null)
            {
                if (cmd.Parameters != null)
                    cmd.Parameters.Clear();

                cmd.Dispose();
            }
            if (conn != null)
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                ConnectionPool.FreeConnection(conn);
            }
        }

        private NpgsqlTransaction _tran = null;
        protected void ExceptionOutPut(NpgsqlCommand cmd, Exception ex)
        {
            string ps = string.Empty;
            if (cmd.Parameters != null)
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    var item = cmd.Parameters[i];
                    ps += $"{ item.ParameterName}:{item.Value},";
                }
            RollBackTransaction();
            Clear(cmd, cmd.Connection);
            _logger.LogError(new EventId(111111), ex, "数据库执行出错：===== \n {0}\n{1}\n{2}", cmd.CommandText, cmd.Parameters, ps);
        }
        public void BeginTransaction()
        {
            if (_tran != null)
                throw new Exception("the transaction is opend");
            Connection = ConnectionPool.GetConnection();
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            _tran = Connection.BeginTransaction();
        }
        public void CommitTransaction()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
            }
            Clear(null, _tran.Connection);
        }
        public void RollBackTransaction()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
            }
        }
    }
}
