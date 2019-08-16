using MyStaging.xUnitTest.Model;
using System;
using Npgsql;
using Microsoft.Extensions.Logging;
using MyStaging.Helpers;
using MyStaging.Common;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;

namespace MyStaging.xUnitTest
{
    public class _startup
    {
        public static void Init(StagingOptions options)
        {
            PgSqlHelper.InitConnection(options);
            Type[] jsonTypes = { typeof(JToken), typeof(JObject), typeof(JArray) };
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet(jsonTypes);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_data_state>("public.et_data_state");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_role>("public.et_role");
        }
    }
}
