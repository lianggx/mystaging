using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.Schemas;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Reflection;

namespace MyStaging.xUnitTest.Model.Schemas
{
	public partial class UserSchema : ISchemaModel
	{
		public static UserSchema Instance => new UserSchema();

		private static Dictionary<string, SchemaModel> schemas { get; }

		public Dictionary<string, SchemaModel> SchemaSet => schemas;

		private static List<PropertyInfo> properties;

		public List<PropertyInfo> Properties => properties;

		static UserSchema()
		{
			schemas = new Dictionary<string, SchemaModel>
			{
				{"id",new SchemaModel{ FieldName="id", DbType= NpgsqlDbType.Varchar, Size=-1, SpecificType=null } },
				{"loginname",new SchemaModel{ FieldName="loginname", DbType= NpgsqlDbType.Varchar, Size=255, SpecificType=null } },
				{"password",new SchemaModel{ FieldName="password", DbType= NpgsqlDbType.Varchar, Size=255, SpecificType=null } },
				{"nickname",new SchemaModel{ FieldName="nickname", DbType= NpgsqlDbType.Varchar, Size=255, SpecificType=null } },
				{"sex",new SchemaModel{ FieldName="sex", DbType= NpgsqlDbType.Boolean, Size=1, SpecificType=null } },
				{"age",new SchemaModel{ FieldName="age", DbType= NpgsqlDbType.Integer, Size=4, SpecificType=null } },
				{"money",new SchemaModel{ FieldName="money", DbType= NpgsqlDbType.Numeric, Size=-1, SpecificType=null } },
				{"createtime",new SchemaModel{ FieldName="createtime", DbType= NpgsqlDbType.Timestamp, Size=8, SpecificType=null } }
			};
			properties = ContractUtils.GetProperties(typeof(UserModel));
		}
	}
}
