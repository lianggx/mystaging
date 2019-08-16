using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.Schemas;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Reflection;

namespace MyStaging.xUnitTest.Model.Schemas
{
	public partial class PostSchema : ISchemaModel
	{
		public static PostSchema Instance => new PostSchema();

		private static Dictionary<string, SchemaModel> schemas { get; }

		public Dictionary<string, SchemaModel> SchemaSet => schemas;

		private static List<PropertyInfo> properties;

		public List<PropertyInfo> Properties => properties;

		static PostSchema()
		{
			schemas = new Dictionary<string, SchemaModel>
			{
				{"id", new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Uuid, Size = 16 ,Primarykey = true} },
				{"title", new SchemaModel{ FieldName = "title", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"content", new SchemaModel{ FieldName = "content", DbType =  NpgsqlDbType.Jsonb, Size = -1} },
				{"state", new SchemaModel{ FieldName = "state", DbType = null, Size = 4} },
				{"role", new SchemaModel{ FieldName = "role", DbType = null, Size = -1} },
				{"text", new SchemaModel{ FieldName = "text", DbType =  NpgsqlDbType.Json, Size = -1} }
			};
			properties = ContractUtils.GetProperties(typeof(PostModel));
		}
	}
}
