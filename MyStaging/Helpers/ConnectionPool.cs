using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库连接池管理对象
    /// </summary>
    public partial class ConnectionPool
    {
        public int PoolSize = 32;
        private object _lock = new object();
        private object _lock_getconnection = new object();
        public List<NpgsqlConnection> All_Connection = new List<NpgsqlConnection>();
        private Queue<ManualResetEvent> GetConnectionQueue = new Queue<ManualResetEvent>();
        private Random connRandom = new Random();

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="connectionList">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public ConnectionPool(string[] connectionList, int poolSize = 32)
        {
            this.ConnectionList = connectionList;
            if (poolSize > 32)
            {
                PoolSize = poolSize;
            }
        }

        /// <summary>
        ///  从连接池中获取可用的数据库连接，如果无法获取，将可能返回 null 
        /// </summary>
        /// <returns></returns>
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
                        string connectionString = RandomConnectionString();
                        conn = new NpgsqlConnection(connectionString);
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

        /// <summary>
        ///  将数据库连接关闭，并放入当前连接池中
        /// </summary>
        /// <param name="conn"></param>
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

        /// <summary>
        ///  获取或者设置数据库连接字符串
        /// </summary>
        public string[] ConnectionList { get; set; }

        /// <summary>
        ///  获取随机的连接
        /// </summary>
        /// <returns></returns>
        private string RandomConnectionString()
        {
            if (ConnectionList.Length == 0)
                return ConnectionList[0];
            else
            {
                int index = connRandom.Next(0, ConnectionList.Length - 1);
                return ConnectionList[index];
            }
        }

        /// <summary>
        ///  获取闲置的连接
        /// </summary>
        public Queue<NpgsqlConnection> Free { get; } = new Queue<NpgsqlConnection>();
    }
}
