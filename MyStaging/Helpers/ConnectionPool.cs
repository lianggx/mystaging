using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MyStaging.Helpers
{
    public partial class ConnectionPool : IDisposable
    {
        public static int Pool_Size = 32;
        private static object _lock_obj = new object();
        private static object _lock_getconnection = new object();
        public static int Connection_Total = 0;
        private static Queue<ManualResetEvent> GetConnectionQueue = new Queue<ManualResetEvent>();
        public static Queue<NpgsqlConnection> Free { get; } = new Queue<NpgsqlConnection>();

        public static string Connection_String { get; set; }

        public static NpgsqlConnection GetConnection()
        {
            NpgsqlConnection conn = null;
            if (Free.Count > 0)
            {
                lock (_lock_obj)
                    if (Free.Count > 0)
                        conn = Free.Dequeue();
            }

            if (conn == null && Connection_Total < Pool_Size)
            {
                lock (_lock_obj)
                    if (Connection_Total < Pool_Size)
                        conn = new NpgsqlConnection(Connection_String);
            }

            if (conn == null)
            {
                ManualResetEvent wait = new ManualResetEvent(false);
                lock (_lock_getconnection)
                {
                    GetConnectionQueue.Enqueue(wait);
                    if (wait.WaitOne(TimeSpan.FromSeconds(10)))
                        GetConnection();
                    else
                        return null;
                }
            }

            Interlocked.Increment(ref Connection_Total);
            return conn;
        }

        private static object _lock_enq_obj = new object();
        public static void FreeConnection(NpgsqlConnection conn)
        {
            lock (_lock_enq_obj)
            {
                Free.Enqueue(conn);
                Interlocked.Decrement(ref Connection_Total);
            }

            if (GetConnectionQueue.Count > 0)
            {
                ManualResetEvent wait = null;
                lock (_lock_getconnection)
                    if (GetConnectionQueue.Count > 0)
                        wait = GetConnectionQueue.Dequeue();
                if (wait != null) wait.Set();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    if (ConnectionPool.Free.Count > 3)
                        lock (_lock_obj)
                            for (int i = 0; i < ConnectionPool.Free.Count - 3; i++)
                            {
                                NpgsqlConnection conn = ConnectionPool.Free.Dequeue();
                                conn.Dispose();
                                Interlocked.Decrement(ref Connection_Total);
                            }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
