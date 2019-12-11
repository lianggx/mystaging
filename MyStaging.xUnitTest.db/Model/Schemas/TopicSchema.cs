using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.Schemas;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Reflection;

namespace MyStaging.xUnitTest.Model.Schemas
{
	public partial class TopicSchema : ISchemaModel
	{
		public static TopicSchema Instance => new TopicSchema();

		public List<SchemaModel> SchemaSet => new List<SchemaModel>
			{
				new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Uuid, Size = 16 ,Primarykey = true},
				new SchemaModel{ FieldName = "title", DbType =  NpgsqlDbType.Varchar, Size = 255},
				new SchemaModel{ FieldName = "create_time", DbType =  NpgsqlDbType.Timestamp, Size = 8},
				new SchemaModel{ FieldName = "update_time", DbType =  NpgsqlDbType.Timestamp, Size = 8},
				new SchemaModel{ FieldName = "last_time", DbType =  NpgsqlDbType.Timestamp, Size = 8},
				new SchemaModel{ FieldName = "user_id", DbType =  NpgsqlDbType.Uuid, Size = 16},
				new SchemaModel{ FieldName = "name", DbType =  NpgsqlDbType.Varchar, Size = 255},
				new SchemaModel{ FieldName = "age", DbType =  NpgsqlDbType.Integer, Size = 4},
				new SchemaModel{ FieldName = "sex", DbType =  NpgsqlDbType.Boolean, Size = 1},
				new SchemaModel{ FieldName = "createtime", DbType =  NpgsqlDbType.Date, Size = 4},
				new SchemaModel{ FieldName = "updatetime", DbType =  NpgsqlDbType.Time, Size = 8}
			};
		public List<PropertyInfo> Properties => ContractUtils.GetProperties(typeof(TopicModel));

	}
}
