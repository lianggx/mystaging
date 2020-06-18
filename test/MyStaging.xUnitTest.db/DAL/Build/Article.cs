using MyStaging.Helpers;
using MyStaging.PostgreSQL;
using MyStaging.xUnitTest.Model;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyStaging.xUnitTest.DAL
{
	public partial class Article : PgDbContext<ArticleModel>
	{
		public static Article Context { get { return new Article(); } }

		public static InsertBuilder<ArticleModel> InsertBuilder => new InsertBuilder<ArticleModel>();
		public static ArticleModel Insert(ArticleModel model) => InsertBuilder.Insert(model);
		public static int InsertRange(List<ArticleModel> models) => InsertBuilder.InsertRange(models).SaveChange();

		public static DeleteBuilder<ArticleModel> DeleteBuilder => new DeleteBuilder<ArticleModel>();
		public static int Delete(string id, string userid)
		{
			var affrows = DeleteBuilder.Where(f => f.Id == id && f.Userid == userid).SaveChange();
			if (affrows > 0) ContextManager.CacheManager?.RemoveItemCache<ArticleModel>(id + "" + userid);
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
			public new ArticleUpdateBuilder Where(string expression)
			{
				base.Where(expression);
				return this;
			}
			public ArticleUpdateBuilder SetId(string id)
			{
				base.SetField("id", id);
				return this;
			}
			public ArticleUpdateBuilder SetUserid(string userid)
			{
				base.SetField("userid", userid);
				return this;
			}
			public ArticleUpdateBuilder SetTitle(string title)
			{
				base.SetField("title", title);
				return this;
			}
			public ArticleUpdateBuilder SetContent(JToken content)
			{
				base.SetField("content", content);
				return this;
			}
			public ArticleUpdateBuilder SetCreatetime(DateTime createtime)
			{
				base.SetField("createtime", createtime);
				return this;
			}
		}

	}
}
