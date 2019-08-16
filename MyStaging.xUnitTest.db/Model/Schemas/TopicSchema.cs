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

		private static Dictionary<string, SchemaModel> schemas { get; }

		public Dictionary<string, SchemaModel> SchemaSet => schemas;

		private static List<PropertyInfo> properties;

		public List<PropertyInfo> Properties => properties;

		static TopicSchema()
		{
			schemas = new Dictionary<string, SchemaModel>
			{
				{"id", new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Uuid, Size = 16 ,Primarykey = true} },
				{"title", new SchemaModel{ FieldName = "title", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"create_time", new SchemaModel{ FieldName = "create_time", DbType =  NpgsqlDbType.Timestamp, Size = 8} },
				{"update_time", new SchemaModel{ FieldName = "update_time", DbType =  NpgsqlDbType.Timestamp, Size = 8} },
				{"last_time", new SchemaModel{ FieldName = "last_time", DbType =  NpgsqlDbType.Timestamp, Size = 8} },
				{"user_id", new SchemaModel{ FieldName = "user_id", DbType =  NpgsqlDbType.Uuid, Size = 16} },
				{"name", new SchemaModel{ FieldName = "name", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"age", new SchemaModel{ FieldName = "age", DbType =  NpgsqlDbType.Integer, Size = 4} },
				{"sex", new SchemaModel{ FieldName = "sex", DbType =  NpgsqlDbType.Boolean, Size = 1} },
				{"createtime", new SchemaModel{ FieldName = "createtime", DbType =  NpgsqlDbType.Date, Size = 4} },
				{"updatetime", new SchemaModel{ FieldName = "updatetime", DbType =  NpgsqlDbType.Time, Size = 8} }
			};
			properties = ContractUtils.GetProperties(typeof(TopicModel));
		}
	}
}
