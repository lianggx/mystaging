using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging;
using MyStaging.Helpers;
using MyStaging.Common;
using NpgsqlTypes;
using System.Linq.Expressions;
using MyStaging.xUnitTest.Model;

namespace MyStaging.xUnitTest.DAL
{
	public partial class User : QueryContext<UserModel>
	{
		const string insertCmdText = "INSERT INTO \"public\".\"user\"(\"id\",\"loginname\",\"password\",\"nickname\",\"sex\",\"age\",\"money\",\"createtime\") VALUES(@id,@loginname,@password,@nickname,@sex,@age,@money,@createtime) RETURNING \"id\",\"loginname\",\"password\",\"nickname\",\"sex\",\"age\",\"money\",\"createtime\";";
		const string deleteCmdText = "DELETE FROM \"public\".\"user\" WHERE \"id\"=@id";
		public static User Context { get { return new User(); } }


		public static UserModel Insert(UserModel model)
		{
			User user = Context;
			user.AddParameter("id", NpgsqlDbType.Varchar, model.Id, -1, null);
			user.AddParameter("loginname", NpgsqlDbType.Varchar, model.Loginname, 255, null);
			user.AddParameter("password", NpgsqlDbType.Varchar, model.Password, 255, null);
			user.AddParameter("nickname", NpgsqlDbType.Varchar, model.Nickname, 255, null);
			user.AddParameter("sex", NpgsqlDbType.Boolean, model.Sex, 1, null);
			user.AddParameter("age", NpgsqlDbType.Integer, model.Age, 4, null);
			user.AddParameter("money", NpgsqlDbType.Numeric, model.Money, -1, null);
			user.AddParameter("createtime", NpgsqlDbType.Timestamp, model.Createtime, 8, null);

			return user.InsertOnReader(insertCmdText);
		}

		public static int Delete(string id)
		{
			User user = Context;
			user.AddParameter("id", NpgsqlDbType.Varchar, id, -1, null);
			return user.ExecuteNonQuery(deleteCmdText);
		}

		public static DeleteBuilder<UserModel> DeleteBuilder { get { return new DeleteBuilder<UserModel>(); } }

		public static UserUpdateBuilder Update(string id)
		{
			return new UserUpdateBuilder(null, id);
		}

		public static UserUpdateBuilder UpdateBuilder { get { return new UserUpdateBuilder(); } }

		public class UserUpdateBuilder : UpdateBuilder<UserModel>
		{
			public UserUpdateBuilder(string id)
			{
				base.Where(f => f.Id == id);
			}

			public UserUpdateBuilder(Action<UserModel> onChanged, string id) : base(onChanged)
			{
				base.Where(f => f.Id == id);
			}

			public UserUpdateBuilder() { }

			public new UserUpdateBuilder Where(Expression<Func<UserModel, bool>> predicate)
			{
				 base.Where(predicate);
				 return this;
			}

			public new UserUpdateBuilder Where(string formatCommad, params object[] pValue)
			{
				 base.Where(formatCommad,pValue);
				 return this;
			}

			public UserUpdateBuilder SetId(string id)
			{
				return base.SetField("id", NpgsqlDbType.Varchar, id, -1, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetLoginname(string loginname)
			{
				return base.SetField("loginname", NpgsqlDbType.Varchar, loginname, 255, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetPassword(string password)
			{
				return base.SetField("password", NpgsqlDbType.Varchar, password, 255, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetNickname(string nickname)
			{
				return base.SetField("nickname", NpgsqlDbType.Varchar, nickname, 255, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetSex(bool? sex)
			{
				return base.SetField("sex", NpgsqlDbType.Boolean, sex, 1, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetAge(int age)
			{
				return base.SetField("age", NpgsqlDbType.Integer, age, 4, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetMoney(decimal money)
			{
				return base.SetField("money", NpgsqlDbType.Numeric, money, -1, null) as UserUpdateBuilder;
			}

			public UserUpdateBuilder SetCreatetime(DateTime createtime)
			{
				return base.SetField("createtime", NpgsqlDbType.Timestamp, createtime, 8, null) as UserUpdateBuilder;
			}

		}

	}
}
