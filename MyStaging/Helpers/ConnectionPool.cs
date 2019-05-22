using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using MyStaging.Common;
using System.Data.Common;
using System.Data;
using Npgsql;

namespace MyStaging.Helpers
{
    /// <summary>
    ///  数据库连接池管理对象
    /// </summary>
    public partial class ConnectionPool
    {
        private readonly object _lock = new object();
        private readonly object _lock_getconnection = new object();
        public readonly List<DbConnection> All_Connection = new List<DbConnection>();
        private readonly Queue<ManualResetEvent> GetConnectionQueue = new Queue<ManualResetEvent>();
        private readonly Random connRandom = new Random();
        private Timer timer = null;

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="connectionList">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public ConnectionPool(List<ConnectionStringConfiguration> connS, int poolSize = 32)
        {
            this.ConnectionList = connS;
            this.PoolSize = poolSize <= 0 ? 32 : poolSize;
        }

        /// <summary>
        ///  从连接池中获取可用的数据库连接，如果无法获取，将抛出异常
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnection()
        {
            DbConnection conn = null;
            if (Free.Count > 0)
                lock (_lock)
                    if (Free.Count > 0)
                        conn = Free.Dequeue();
            if (conn == null && All_Connection.Count < this.PoolSize)
            {
                lock (_lock)
                    if (All_Connection.Count < this.PoolSize)
                    {
                        var connS = RandomConnectionString();
                        conn = new NpgsqlConnection(connS.ConnectionString);

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
                throw new TimeoutException("从连接池中获取数据库连接超时，可能已无可用连接");
            }

            return conn;
        }

        /// <summary>
        ///  将数据库连接关闭，并放入当前连接池中
        /// </summary>
        /// <param name="conn"></param>
        public void FreeConnection(DbConnection conn)
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
        ///  获取随机的连接
        /// </summary>
        /// <returns></returns>
        private ConnectionStringConfiguration RandomConnectionString()
        {
            if (ConnectionList.Count == 0)
            {
                throw new NoSlaveConnection("数据库连接数量为 0 ，无法创建连接");
            }
            if (ConnectionList.Count == 1)
            {
                return ConnectionList[0];
            }
            else
            {
                var connS = ConnectionList.Where(f => f.MaxConnection > f.Used).OrderBy(f => f.Used).First();
                if (connS == null)
                    throw new NoSlaveConnection("已无可用的数据库连接");

                connS.Used++;
                return connS;
            }
        }

        /// <summary>
        ///  移除异常的数据库连接
        /// </summary>
        /// <param name="connectionString"></param>
        public void RemoveConnection(DbConnection dbConnection)
        {
            for (int i = 0; i < this.ConnectionList.Count; i++)
            {
                var sourceConfig = this.ConnectionList[i];
                var assert = sourceConfig.DbConnection.DataSource == dbConnection.DataSource
                            && sourceConfig.DbConnection.Database == dbConnection.Database;
                if (assert)
                {
                    this.ConnectionList.RemoveAt(i);
                    Monitor(sourceConfig);
                    break;
                }
            }
        }

        /// <summary>
        ///  启动连接监控
        /// </summary>
        /// <param name="connectionString"></param>
        private void Monitor(ConnectionStringConfiguration connS)
        {
            ErrorList.Add(connS);
            if (timer == null)
            {
                timer = new Timer(OnTick, this, 10 * 1000, 60 * 1000);
                Console.WriteLine("监控服务已启动");
            }
        }

        /// <summary>
        ///  检查连接是否正常
        /// </summary>
        /// <param name="state"></param>
        private void OnTick(object state)
        {
            if (ErrorList.Count == 0)
                return;

            for (int i = 0; i < ErrorList.Count; i++)
            {
                var connS = ErrorList[i];
                try
                {
                    ErrorList[i].Error++;
                    NpgsqlConnection conn = new NpgsqlConnection(connS.ConnectionString);
                    conn.Open();
                    conn.Close();
                    ErrorList.RemoveAt(i);
                    ConnectionList.Add(connS);

                    Console.WriteLine("连接正常，重新放入连接池：[{0}]", connS.ConnectionString);
                }
                catch { }
            }
        }

        /// <summary>
        /// 刷新数据库连接
        /// </summary>
        /// <param name="connS"></param>
        /// <param name="poolSize"></param>
        public void Refresh(List<ConnectionStringConfiguration> connS, int poolSize = 32)
        {
            lock (_lock)
            {
                ConnectionList?.Clear();
                ConnectionList = connS;
                this.PoolSize = poolSize;
                Free?.Clear();
                All_Connection?.Clear();
            }
        }

        /// <summary>
        ///  获取连接池大小
        /// </summary>
        public int PoolSize { get; set; } = 32;

        /// <summary>
        ///  获取闲置的连接
        /// </summary>
        public Queue<DbConnection> Free { get; } = new Queue<DbConnection>();

        /// <summary>
        ///  获取或者设置数据库连接字符串
        /// </summary>
        public List<ConnectionStringConfiguration> ConnectionList { get; private set; } = new List<ConnectionStringConfiguration>();

        /// <summary>
        ///  获取或者设置连接异常的数据库连接
        /// </summary>
        public List<ConnectionStringConfiguration> ErrorList { get; private set; } = new List<ConnectionStringConfiguration>();
    }

    /// <summary>
    ///  未配置从库连接时抛出该异常
    /// </summary>
    public class NoSlaveConnection : Exception
    {
        public NoSlaveConnection() { }
        public NoSlaveConnection(string message) : base(message) { }
    }
}
