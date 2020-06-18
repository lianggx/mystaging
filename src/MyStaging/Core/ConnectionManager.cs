using MyStaging.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyStaging.Core
{
    public class ConnectionManager
    {
        public static ConcurrentDictionary<string, List<ConnectionModel>> dict = new ConcurrentDictionary<string, List<ConnectionModel>>();

        public static void Add(string name, string connectionString, bool readOnly)
        {
            var conn = new ConnectionModel
            {
                ConnectionString = connectionString,
                ReadOnly = readOnly
            };

            if (dict.ContainsKey(name))
            {
                dict[name].Add(conn);
            }
            else
            {
                var models = new List<ConnectionModel>()
                {
                    conn
                };
                dict.TryAdd(name, models);
            }
        }

        public static ConnectionModel Get(string name, bool readOnly)
        {
            dict.TryGetValue(name, out List<ConnectionModel> models);
            if (models == null || models.Count == 0)
            {
                throw new InvalidOperationException("已无可用的数据库连接");
            }

            ConnectionModel connection = models.Count == 1 ? models[0] : models.OrderBy(f => f.Used).First();

            if (connection.Used < long.MaxValue)
            {
                connection.Used++;
            }

            return connection;
        }

        public static void Remove(string name)
        {
            dict.TryRemove(name, out _);
        }
    }
}
