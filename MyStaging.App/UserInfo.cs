using MyStaging.Helpers;
using MyStaging.Mapping;
using NpgsqlTypes;
using System;

namespace MyStaging.App.Test
{
    [EntityMapping(TableName = "public.app")]
    public class public_app
    {
        public Guid id { get; set; }
        public Guid guser_id { get; set; }
        public string title { get; set; }

    }

    [EntityMapping(TableName = "public.guser")]
    public class UserModel
    {
        private Guid _id;
        [PropertyMapping(DbType = NpgsqlDbType.Uuid, FieldName = "id", Length = 32)]
        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }
        public string NickName { get; set; }
    }

    public partial class UserDal : QueryContext<UserModel>
    {
        private const string deleteCmdText = "";
        private const string insertCmdText = "";

        public static UserModel Insert(UserModel model)
        {
            UserDal userDal = Context;
            //userDal.SetParameter("title", NpgsqlDbType.Varchar, model.Title);
            //userDal.SetParameter("type", NpgsqlDbType.Enum, model.Type);
            //userDal.SetParameter("author_id", NpgsqlDbType.Uuid, model.Author_id);
            //userDal.SetParameter("create_time", NpgsqlDbType.Timestamp, model.Create_time);
            //userDal.SetParameter("id", NpgsqlDbType.Uuid, model.Id);
            //userDal.SetParameter("parent_id", NpgsqlDbType.Uuid, model.Parent_id);
            //userDal.SetParameter("content", NpgsqlDbType.Text, model.Content);
            //userDal.SetParameter("subtitle", NpgsqlDbType.Varchar, model.Subtitle);
            //userDal.SetParameter("app_id", NpgsqlDbType.Uuid, model.App_id);
            //userDal.SetParameter("state", NpgsqlDbType.Enum, model.State);
            return userDal.InsertOnReader(insertCmdText);
        }

        public static UserUpdateBuilder Update(Guid id)
        {
            return new UserUpdateBuilder(id);
        }

        public static int Delete(Guid id)
        {
            return Context.AddParameter("id", NpgsqlDbType.Uuid, id).ExecuteNonQuery(deleteCmdText);
        }
        private static UserDal Context { get { return new UserDal(); } }

        #region Properties
        public class UserUpdateBuilder : UpdateBuilder<UserModel>
        {
            public UserUpdateBuilder(Guid id)
            {
                base.AddParameter("id", NpgsqlDbType.Uuid, id);
            }

            public UserUpdateBuilder SetNickName(string nickname)
            {
                return base.SetField("nickname", NpgsqlDbType.Varchar, nickname) as UserUpdateBuilder;
            }
        }

        #endregion
    }
}

