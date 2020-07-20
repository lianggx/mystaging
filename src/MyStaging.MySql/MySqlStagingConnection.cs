using MySql.Data.MySqlClient;
using MyStaging.Core;
using MyStaging.Interface;
using System.Data.Common;

namespace MyStaging.MySql
{
    public partial class MySqlStagingConnection : IStagingConnection
    {
        public DbConnection GetConnection(string name, bool readOnly)
        {
            var model = ConnectionManager.Get(name, readOnly);
            var conn = new MySqlConnection(model.ConnectionString);

            return conn;
        }

        public void Refresh(string name, string master, params string[] slaves)
        {
            MySqlConnection.ClearAllPools();
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
