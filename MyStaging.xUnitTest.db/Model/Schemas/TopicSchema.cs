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
				{"id", new SchemaModel{ FieldName = "id", DbType = NpgsqlDbType.Uuid, Size = 16, SpecificType = null ,Primarykey = true} },
				{"title", new SchemaModel{ FieldName = "title", DbType = NpgsqlDbType.Varchar, Size = 255, SpecificType = null} },
				{"create_time", new SchemaModel{ FieldName = "create_time", DbType = NpgsqlDbType.Timestamp, Size = 8, SpecificType = null} },
				{"update_time", new SchemaModel{ FieldName = "update_time", DbType = NpgsqlDbType.Timestamp, Size = 8, SpecificType = null} },
				{"last_time", new SchemaModel{ FieldName = "last_time", DbType = NpgsqlDbType.Timestamp, Size = 8, SpecificType = null} },
				{"user_id", new SchemaModel{ FieldName = "user_id", DbType = NpgsqlDbType.Uuid, Size = 16, SpecificType = null} },
				{"name", new SchemaModel{ FieldName = "name", DbType = NpgsqlDbType.Varchar, Size = 255, SpecificType = null} },
				{"age", new SchemaModel{ FieldName = "age", DbType = NpgsqlDbType.Integer, Size = 4, SpecificType = null} },
				{"sex", new SchemaModel{ FieldName = "sex", DbType = NpgsqlDbType.Boolean, Size = 1, SpecificType = null} },
				{"createtime", new SchemaModel{ FieldName = "createtime", DbType = NpgsqlDbType.Date, Size = 4, SpecificType = null} },
				{"updatetime", new SchemaModel{ FieldName = "updatetime", DbType = NpgsqlDbType.Time, Size = 8, SpecificType = null} }
			};
			properties = ContractUtils.GetProperties(typeof(TopicModel));
		}
	}
}
