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
	public partial class Article : QueryContext<ArticleModel>
	{
		const string insertCmdText = "INSERT INTO \"public\".\"article\"(\"id\",\"userid\",\"title\",\"content\",\"createtime\") VALUES(@id,@userid,@title,@content,@createtime) RETURNING \"id\",\"userid\",\"title\",\"content\",\"createtime\";";
		const string deleteCmdText = "DELETE FROM \"public\".\"article\" WHERE \"id\"=@id";
		public static Article Context { get { return new Article(); } }


		public static ArticleModel Insert(ArticleModel model)
		{
			Article article = Context;
			article.AddParameter("id", NpgsqlDbType.Varchar, model.Id, -1, null);
			article.AddParameter("userid", NpgsqlDbType.Varchar, model.Userid, -1, null);
			article.AddParameter("title", NpgsqlDbType.Varchar, model.Title, 255, null);
			article.AddParameter("content", NpgsqlDbType.Jsonb, model.Content, -1, null);
			article.AddParameter("createtime", NpgsqlDbType.Timestamp, model.Createtime, 8, null);

			return article.InsertOnReader(insertCmdText);
		}

		public static int Delete(string id)
		{
			Article article = Context;
			article.AddParameter("id", NpgsqlDbType.Varchar, id, -1, null);
			return article.ExecuteNonQuery(deleteCmdText);
		}

		public static DeleteBuilder<ArticleModel> DeleteBuilder { get { return new DeleteBuilder<ArticleModel>(); } }

		public static ArticleUpdateBuilder Update(string id)
		{
			return new ArticleUpdateBuilder(null, id);
		}

		public static ArticleUpdateBuilder UpdateBuilder { get { return new ArticleUpdateBuilder(); } }

		public class ArticleUpdateBuilder : UpdateBuilder<ArticleModel>
		{
			public ArticleUpdateBuilder(string id)
			{
				base.Where(f => f.Id == id);
			}

			public ArticleUpdateBuilder(Action<ArticleModel> onChanged, string id) : base(onChanged)
			{
				base.Where(f => f.Id == id);
			}

			public ArticleUpdateBuilder() { }

			public new ArticleUpdateBuilder Where(Expression<Func<ArticleModel, bool>> predicate)
			{
				 base.Where(predicate);
				 return this;
			}

			public new ArticleUpdateBuilder Where(string formatCommad, params object[] pValue)
			{
				 base.Where(formatCommad,pValue);
				 return this;
			}

			public ArticleUpdateBuilder SetId(string id)
			{
				return base.SetField("id", NpgsqlDbType.Varchar, id, -1, null) as ArticleUpdateBuilder;
			}

			public ArticleUpdateBuilder SetUserid(string userid)
			{
				return base.SetField("userid", NpgsqlDbType.Varchar, userid, -1, null) as ArticleUpdateBuilder;
			}

			public ArticleUpdateBuilder SetTitle(string title)
			{
				return base.SetField("title", NpgsqlDbType.Varchar, title, 255, null) as ArticleUpdateBuilder;
			}

			public ArticleUpdateBuilder SetContent(JToken content)
			{
				return base.SetField("content", NpgsqlDbType.Jsonb, content, -1, null) as ArticleUpdateBuilder;
			}

			public ArticleUpdateBuilder SetCreatetime(DateTime createtime)
			{
				return base.SetField("createtime", NpgsqlDbType.Timestamp, createtime, 8, null) as ArticleUpdateBuilder;
			}

		}

	}
}
