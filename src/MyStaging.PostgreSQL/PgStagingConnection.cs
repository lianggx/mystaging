using MyStaging.Core;
using MyStaging.Interface;
using Npgsql;
using System.Data.Common;

namespace MyStaging.PostgreSQL
{
    public partial class PgStagingConnection : IStagingConnection
    {
        public DbConnection GetConnection(string name, bool readOnly)
        {
            var model = ConnectionManager.Get(name, readOnly);
            var conn = new NpgsqlConnection(model.ConnectionString);

            return conn;
        }

        public void Refresh(string name, string master, params string[] slaves)
        {
            NpgsqlConnection.ClearAllPools();
            ConnectionManager.Remove(name);
            ConnectionManager.Add(name, master, false);
            if (slaves?.Length > 0)
            {
                foreach (var conn in slaves)
                {
                    ConnectionManager.Add(name, conn, true);
                }
            }
        }
    }
}
