using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.Schemas;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Reflection;

namespace MyStaging.xUnitTest.Model.Schemas
{
	public partial class ArticleSchema : ISchemaModel
	{
		public static ArticleSchema Instance => new ArticleSchema();

		private static Dictionary<string, SchemaModel> schemas { get; }

		public Dictionary<string, SchemaModel> SchemaSet => schemas;

		private static List<PropertyInfo> properties;

		public List<PropertyInfo> Properties => properties;

		static ArticleSchema()
		{
			schemas = new Dictionary<string, SchemaModel>
			{
				{"id", new SchemaModel{ FieldName = "id", DbType = NpgsqlDbType.Varchar, Size = -1, SpecificType = null ,Primarykey = true} },
				{"userid", new SchemaModel{ FieldName = "userid", DbType = NpgsqlDbType.Varchar, Size = -1, SpecificType = null ,Primarykey = true} },
				{"title", new SchemaModel{ FieldName = "title", DbType = NpgsqlDbType.Varchar, Size = 255, SpecificType = null} },
				{"content", new SchemaModel{ FieldName = "content", DbType = NpgsqlDbType.Jsonb, Size = -1, SpecificType = null} },
				{"createtime", new SchemaModel{ FieldName = "createtime", DbType = NpgsqlDbType.Timestamp, Size = 8, SpecificType = null} }
			};
			properties = ContractUtils.GetProperties(typeof(ArticleModel));
		}
	}
}
