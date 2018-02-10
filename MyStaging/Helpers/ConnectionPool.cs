using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MyStaging.Helpers
{
    public partial class ConnectionPool
    {
        public int PoolSize = 32;
        private object _lock = new object();
        private object _lock_getconnection = new object();
        public List<NpgsqlConnection> All_Connection = new List<NpgsqlConnection>();
        private Queue<ManualResetEvent> GetConnectionQueue = new Queue<ManualResetEvent>();

        public ConnectionPool(string connectionString, int poolSize = 32)
        {
            this.ConnectionString = connectionString;
            if (poolSize > 32)
            {
                PoolSize = poolSize;
            }
        }

        public NpgsqlConnection GetConnection()
        {
            NpgsqlConnection conn = null;
            if (Free.Count > 0)
                lock (_lock)
                    if (Free.Count > 0)
                        conn = Free.Dequeue();
            if (conn == null && All_Connection.Count < PoolSize)
            {
                lock (_lock)
                    if (All_Connection.Count < PoolSize)
                    {
                        conn = new NpgsqlConnection(ConnectionString);
                        All_Connection.Add(conn);
                    }
            }

            if (conn == null)
            {
                ManualResetEvent wait = new ManualResetEvent(false);
                lock (_lock_getconnection)
                    GetConnectionQueue.Enqueue(wait);
                if (wait.WaitOne(TimeSpan.FromSeconds(10)))
                    return GetConnection();
                return null;
            }

            return conn;
        }

        public void FreeConnection(NpgsqlConnection conn)
        {
            conn.Close();
            lock (_lock)
                Free.Enqueue(conn);

            if (GetConnectionQueue.Count > 0)
            {
                ManualResetEvent wait = null;
                lock (_lock_getconnection)
                    if (GetConnectionQueue.Count > 0)
                        wait = GetConnectionQueue.Dequeue();
                if (wait != null) wait.Set();
            }
        }

        public string ConnectionString { get; set; }
        public Queue<NpgsqlConnection> Free { get; } = new Queue<NpgsqlConnection>();
    }
}
