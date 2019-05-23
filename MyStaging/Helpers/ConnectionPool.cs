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
using System.Threading.Tasks;

namespace MyStaging.Helpers
{
    public class EasyLock
    {
        private ReaderWriterLockSlim slim = new ReaderWriterLockSlim();
        public bool Enabled { get; set; }

        public EasyLock()
        {
            Enabled = true;
        }

        public IDisposable Read()
        {
            if (Enabled == false || slim.IsReadLockHeld || slim.IsWriteLockHeld)
            {
                return Disposable.Empty;
            }
            else
            {
                slim.EnterReadLock();
                return new SimpleLock(slim, false);
            }
        }

        public IDisposable Write()
        {
            if (Enabled == false || slim.IsWriteLockHeld)
            {
                return Disposable.Empty;
            }
            else if (slim.IsReadLockHeld)
            {
                throw new NotImplementedException("读取模式不允许切换到写入模式锁");
            }
            else
            {
                slim.EnterWriteLock();
                return new SimpleLock(slim, true);
            }
        }

        private class SimpleLock : IDisposable
        {
            private ReaderWriterLockSlim slimLock;

            private bool writing;
            public SimpleLock(ReaderWriterLockSlim slimLock, bool writing)
            {
                this.slimLock = slimLock;
                this.writing = writing;
            }

            public void Dispose()
            {
                if (writing)
                {
                    if (slimLock.IsWriteLockHeld)
                    {
                        slimLock.ExitWriteLock();
                    }
                }
                else
                {
                    if (slimLock.IsReadLockHeld)
                    {
                        slimLock.ExitReadLock();
                    }
                }
            }
        }

        private class Disposable : IDisposable
        {
            public static readonly Disposable Empty = new Disposable();
            public void Dispose() { }
        }
    }

    /// <summary>
    ///  数据库连接池管理对象
    /// </summary>
    public partial class ConnectionPool
    {
        private EasyLock easyLock = null;
        private Timer timer = null;

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="connectionList">数据库连接字符串</param>
        /// <param name="poolSize">连接池大小</param>
        public ConnectionPool(List<ConnectionStringConfiguration> connS, int poolSize = 32)
        {
            this.ConnectionList = connS;
            this.easyLock = new EasyLock();
        }

        private object objLock = new object();
        /// <summary>
        ///  从连接池中获取可用的数据库连接，如果无法获取，将抛出异常
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnection()
        {
            using (easyLock.Write())
            {
                var connS = RandomConnectionString();
                var conn = new NpgsqlConnection(connS.ConnectionString);
                return conn;
            }
        }

        /// <summary>
        ///  将数据库连接关闭，并放入当前连接池中
        /// </summary>
        /// <param name="conn"></param>
        public void FreeConnection(DbConnection conn)
        {
            conn.Close();
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
                ConnectionList[0].Used++;
                return ConnectionList[0];
            }
            else
            {
                var connS = ConnectionList.OrderBy(f => f.Used).First();
                if (connS == null)
                    throw new NoSlaveConnection("已无可用的数据库连接");

                if (connS.Used < long.MaxValue)
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
            using (easyLock.Write())
            {
                NpgsqlConnection.ClearAllPools();
                ConnectionList?.Clear();
                ConnectionList = connS;
            }
        }

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
