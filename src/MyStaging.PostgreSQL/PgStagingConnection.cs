using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Interface;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

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
            foreach (var conn in slaves)
            {
                ConnectionManager.Add(name, conn, true);
            }
        }
    }
}
