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
        public static int Connection_Total = 0;
        public static Queue<NpgsqlConnection> Free { get; } = new Queue<NpgsqlConnection>();

        public static string Connection_String { get; set; }

        public static NpgsqlConnection GetConnection()
        {
            Console.WriteLine(Connection_Total);

            if (Free.Count == 0)
            {
                lock (_lock_obj)
                {
                    if (Connection_Total >= Pool_Size)
                    {
                        ManualResetEvent manual = new ManualResetEvent(false);
                        manual.WaitOne(TimeSpan.FromSeconds(5));
                        return GetConnection();
                    }
                    Interlocked.Increment(ref Connection_Total);
                    NpgsqlConnection cm = new NpgsqlConnection(Connection_String);
                    Free.Enqueue(cm);
                }
            }

            return Free.Dequeue();
        }

        private static object _lock_enq_obj = new object();
        public static void FreeConnection(NpgsqlConnection conn)
        {
            lock (_lock_enq_obj)
            {
                Free.Enqueue(conn);
                Interlocked.Decrement(ref Connection_Total);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ConnectionPool.Free.Count > 3)
                    {
                        lock (_lock_obj)
                        {
                            for (int i = 0; i < ConnectionPool.Free.Count - 3; i++)
                            {
                                NpgsqlConnection conn = ConnectionPool.Free.Dequeue();
                                conn.Dispose();
                                Interlocked.Decrement(ref Connection_Total);
                            }
                        }
                    }

                    disposedValue = true;
                }
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
