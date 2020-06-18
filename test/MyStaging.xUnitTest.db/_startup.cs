using MyStaging.xUnitTest.Model;
using System;
using Npgsql;
using Microsoft.Extensions.Logging;
using MyStaging.Helpers;
using MyStaging.Common;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Distributed;

namespace MyStaging.xUnitTest
{
	public class _startup
	{
		public static void Init(StagingOptions options)
		{
			ContextManager.InitConnection(options);

			Type[] jsonTypes = { typeof(JToken), typeof(JObject), typeof(JArray) };
			NpgsqlNameTranslator translator = new NpgsqlNameTranslator();
			NpgsqlConnection.GlobalTypeMapper.UseJsonNet(jsonTypes);

			NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_data_state>("public.et_data_state", translator);
			NpgsqlConnection.GlobalTypeMapper.MapEnum<Et_role>("public.et_role", translator);
		}
	}
	public partial class NpgsqlNameTranslator : INpgsqlNameTranslator
	{
		public string TranslateMemberName(string clrName) => clrName;
		public string TranslateTypeName(string clrTypeName) => clrTypeName;
	}
}
