using MyStaging.Helpers;
using MyStaging.PostgreSQL;
using MyStaging.xUnitTest.Model;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyStaging.xUnitTest.DAL
{
	public partial class User : PgDbContext<UserModel>
	{
		public static User Context { get { return new User(); } }

		public static InsertBuilder<UserModel> InsertBuilder => new InsertBuilder<UserModel>();
		public static UserModel Insert(UserModel model) => InsertBuilder.Insert(model);
		public static int InsertRange(List<UserModel> models) => InsertBuilder.InsertRange(models).SaveChange();

		public static DeleteBuilder<UserModel> DeleteBuilder => new DeleteBuilder<UserModel>();
		public static int Delete(string id)
		{
			var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
			if (affrows > 0) ContextManager.CacheManager?.RemoveItemCache<UserModel>(id);
			return affrows;
		}

		public static UserUpdateBuilder UpdateBuilder => new UserUpdateBuilder();
		public static UserUpdateBuilder Update(string id)
		{
			return new UserUpdateBuilder(null, id);
		}

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
			public new UserUpdateBuilder Where(string expression)
			{
				base.Where(expression);
				return this;
			}
			public UserUpdateBuilder SetId(string id)
			{
				base.SetField("id", id);
				return this;
			}
			public UserUpdateBuilder SetLoginname(string loginname)
			{
				base.SetField("loginname", loginname);
				return this;
			}
			public UserUpdateBuilder SetPassword(string password)
			{
				base.SetField("password", password);
				return this;
			}
			public UserUpdateBuilder SetNickname(string nickname)
			{
				base.SetField("nickname", nickname);
				return this;
			}
			public UserUpdateBuilder SetSex(bool? sex)
			{
				base.SetField("sex", sex);
				return this;
			}
			public UserUpdateBuilder SetAge(int age)
			{
				base.SetField("age", age);
				return this;
			}
			public UserUpdateBuilder SetMoney(decimal money)
			{
				base.SetField("money", money);
				return this;
			}
			public UserUpdateBuilder SetCreatetime(DateTime createtime)
			{
				base.SetField("createtime", createtime);
				return this;
			}
			public UserUpdateBuilder SetWealth(decimal wealth)
			{
				base.SetField("wealth", wealth);
				return this;
			}
			public UserUpdateBuilder SetRole(Et_role? role)
			{
				base.SetField("role", role);
				return this;
			}
		}

	}
}
