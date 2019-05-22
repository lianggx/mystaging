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
    public partial class Article : QueryContext<ArticleModel>
    {
        public static Article Context { get { return new Article(); } }

        public static InsertBuilder<ArticleModel> InsertBuilder => new InsertBuilder<ArticleModel>(ArticleSchema.Instance);
        public static ArticleModel Insert(ArticleModel model) => InsertBuilder.Insert(model);
        public static int InsertRange(List<ArticleModel> models) => InsertBuilder.InsertRange(models).SaveChange();

        public static DeleteBuilder<ArticleModel> DeleteBuilder => new DeleteBuilder<ArticleModel>();
        public static int Delete(string id, string userid)
        {
            var affrows = DeleteBuilder.Where(f => f.Id == id && f.Userid == userid).SaveChange();
            if (affrows > 0) Helpers.PgSqlHelper.CacheManager?.RemoveItemCache<ArticleModel>(id);
            return affrows;
        }

        public static ArticleUpdateBuilder UpdateBuilder => new ArticleUpdateBuilder();
        public static ArticleUpdateBuilder Update(string id, string userid)
        {
            return new ArticleUpdateBuilder(null, id, userid);
        }

        public class ArticleUpdateBuilder : UpdateBuilder<ArticleModel>
        {
            public ArticleUpdateBuilder(string id, string userid)
            {
                base.Where(f => f.Id == id && f.Userid == userid);
            }

            public ArticleUpdateBuilder(Action<ArticleModel> onChanged, string id, string userid) : base(onChanged)
            {
                base.Where(f => f.Id == id && f.Userid == userid);
            }

            public ArticleUpdateBuilder() { }

            public new ArticleUpdateBuilder Where(Expression<Func<ArticleModel, bool>> predicate)
            {
                base.Where(predicate);
                return this;
            }
            public new ArticleUpdateBuilder Where(string formatCommad, params object[] pValue)
            {
                base.Where(formatCommad, pValue);
                return this;
            }
            public ArticleUpdateBuilder SetId(string id)
            {
                base.SetField("id", NpgsqlDbType.Varchar, id, -1, null);
                return this;
            }
            public ArticleUpdateBuilder SetUserid(string userid)
            {
                base.SetField("userid", NpgsqlDbType.Varchar, userid, -1, null);
                return this;
            }
            public ArticleUpdateBuilder SetTitle(string title)
            {
                base.SetField("title", NpgsqlDbType.Varchar, title, 255, null);
                return this;
            }
            public ArticleUpdateBuilder SetContent(JToken content)
            {
                base.SetField("content", NpgsqlDbType.Jsonb, content, -1, null);
                return this;
            }
            public ArticleUpdateBuilder SetCreatetime(DateTime createtime)
            {
                base.SetField("createtime", NpgsqlDbType.Timestamp, createtime, 8, null);
                return this;
            }
        }

    }
}
