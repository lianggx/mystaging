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

		public List<SchemaModel> SchemaSet => new List<SchemaModel>
			{
				new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Uuid, Size = 16 ,Primarykey = true},
				new SchemaModel{ FieldName = "title", DbType =  NpgsqlDbType.Varchar, Size = 255},
				new SchemaModel{ FieldName = "content", DbType =  NpgsqlDbType.Jsonb, Size = -1},
				new SchemaModel{ FieldName = "state", DbType = null, Size = 4},
				new SchemaModel{ FieldName = "role", DbType = null, Size = 4},
				new SchemaModel{ FieldName = "text", DbType =  NpgsqlDbType.Json, Size = -1}
			};
		public List<PropertyInfo> Properties => ContractUtils.GetProperties(typeof(PostModel));

	}
}
