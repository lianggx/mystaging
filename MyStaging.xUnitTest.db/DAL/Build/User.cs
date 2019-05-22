using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyStaging;
using MyStaging.Helpers;
using MyStaging.Common;
using NpgsqlTypes;
using System.Linq.Expressions;
using System.Collections.Generic;
using MyStaging.xUnitTest.Model;
using MyStaging.xUnitTest.Model.Schemas;

namespace MyStaging.xUnitTest.DAL
{
    public partial class User : QueryContext<UserModel>
    {
        public static User Context { get { return new User(); } }

        public static InsertBuilder<UserModel> InsertBuilder => new InsertBuilder<UserModel>(UserSchema.Instance);
        public static UserModel Insert(UserModel model) => InsertBuilder.Insert(model);
        public static int InsertRange(List<UserModel> models) => InsertBuilder.InsertRange(models).SaveChange();

        public static DeleteBuilder<UserModel> DeleteBuilder => new DeleteBuilder<UserModel>();
        public static int Delete(string id)
        {
            var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
            if (affrows > 0) Helpers.PgSqlHelper.CacheManager?.RemoveItemCache<UserModel>(id);
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
            public new UserUpdateBuilder Where(string formatCommad, params object[] pValue)
            {
                base.Where(formatCommad, pValue);
                return this;
            }
            public UserUpdateBuilder SetId(string id)
            {
                base.SetField("id", NpgsqlDbType.Varchar, id, -1, null);
                return this;
            }
            public UserUpdateBuilder SetLoginname(string loginname)
            {
                base.SetField("loginname", NpgsqlDbType.Varchar, loginname, 255, null);
                return this;
            }
            public UserUpdateBuilder SetPassword(string password)
            {
                base.SetField("password", NpgsqlDbType.Varchar, password, 255, null);
                return this;
            }
            public UserUpdateBuilder SetNickname(string nickname)
            {
                base.SetField("nickname", NpgsqlDbType.Varchar, nickname, 255, null);
                return this;
            }
            public UserUpdateBuilder SetSex(bool? sex)
            {
                base.SetField("sex", NpgsqlDbType.Boolean, sex, 1, null);
                return this;
            }
            public UserUpdateBuilder SetAge(int age)
            {
                base.SetField("age", NpgsqlDbType.Integer, age, 4, null);
                return this;
            }
            public UserUpdateBuilder SetMoney(decimal money)
            {
                base.SetField("money", NpgsqlDbType.Numeric, money, -1, null);
                return this;
            }
            public UserUpdateBuilder SetCreatetime(DateTime createtime)
            {
                base.SetField("createtime", NpgsqlDbType.Timestamp, createtime, 8, null);
                return this;
            }
        }

    }
}
