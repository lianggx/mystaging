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




		public Dictionary<string, SchemaModel> SchemaSet => new Dictionary<string, SchemaModel>
			{
				{"id", new SchemaModel{ FieldName = "id", DbType =  NpgsqlDbType.Varchar, Size = -1 ,Primarykey = true} },
				{"loginname", new SchemaModel{ FieldName = "loginname", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"password", new SchemaModel{ FieldName = "password", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"nickname", new SchemaModel{ FieldName = "nickname", DbType =  NpgsqlDbType.Varchar, Size = 255} },
				{"sex", new SchemaModel{ FieldName = "sex", DbType =  NpgsqlDbType.Boolean, Size = 1} },
				{"age", new SchemaModel{ FieldName = "age", DbType =  NpgsqlDbType.Integer, Size = 4} },
				{"money", new SchemaModel{ FieldName = "money", DbType =  NpgsqlDbType.Numeric, Size = -1} },
				{"createtime", new SchemaModel{ FieldName = "createtime", DbType =  NpgsqlDbType.Timestamp, Size = 8} }
			};
		public List<PropertyInfo> Properties => ContractUtils.GetProperties(typeof(UserModel));

	}
}
