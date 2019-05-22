using MyStaging.xUnitTest.Model;
using System;
using Npgsql;
using Microsoft.Extensions.Logging;
using MyStaging.Helpers;
using MyStaging.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace MyStaging.xUnitTest
{
	public class _startup
	{
		public static void Init(StagingOptions options)
		{
			PgSqlHelper.InitConnection(options);
		}
	}
	public partial class NpgsqlNameTranslator : INpgsqlNameTranslator
	{
		private string clrName;
		public string TranslateMemberName(string clrName)
		{
			this.clrName = clrName;
			return this.clrName;
		}
		private string clrTypeName;
		public string TranslateTypeName(string clrName)
		{
			this.clrTypeName = clrName;
			return this.clrTypeName;
		}
	}
}
