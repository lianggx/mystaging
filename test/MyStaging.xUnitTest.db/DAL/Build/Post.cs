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
	public partial class Post : QueryContext<PostModel>
	{
		public static Post Context { get { return new Post(); } }

		public static InsertBuilder<PostModel> InsertBuilder => new InsertBuilder<PostModel>(PostSchema.Instance);
		public static PostModel Insert(PostModel model) => InsertBuilder.Insert(model);
		public static int InsertRange(List<PostModel> models) => InsertBuilder.InsertRange(models).SaveChange();

		public static DeleteBuilder<PostModel> DeleteBuilder => new DeleteBuilder<PostModel>();
		public static int Delete(Guid id)
		{
			var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
			if (affrows > 0) PgSqlHelper.CacheManager?.RemoveItemCache<PostModel>(id.ToString());
			return affrows;
			}

		public static PostUpdateBuilder UpdateBuilder => new PostUpdateBuilder();
		public static PostUpdateBuilder Update(Guid id)
		{
			return new PostUpdateBuilder(null, id);
		}

		public class PostUpdateBuilder : UpdateBuilder<PostModel>
		{
			public PostUpdateBuilder(Guid id)
			{
				base.Where(f => f.Id == id);
			}

			public PostUpdateBuilder(Action<PostModel> onChanged, Guid id) : base(onChanged)
			{
				base.Where(f => f.Id == id);
			}

			public PostUpdateBuilder() { }

			public new PostUpdateBuilder Where(Expression<Func<PostModel, bool>> predicate)
			{
				base.Where(predicate);
				return this;
			}
			public new PostUpdateBuilder Where(string expression)
			{
				base.Where(expression);
				return this;
			}
			public PostUpdateBuilder SetId(Guid id)
			{
				base.SetField("id", NpgsqlDbType.Uuid, id, 16);
				return this;
			}
			public PostUpdateBuilder SetTitle(string title)
			{
				base.SetField("title", NpgsqlDbType.Varchar, title, 255);
				return this;
			}
			public PostUpdateBuilder SetContent(JToken content)
			{
				base.SetField("content", NpgsqlDbType.Jsonb, content, -1);
				return this;
			}
			public PostUpdateBuilder SetState(Et_data_state? state)
			{
				base.SetField("state", state, 4);
				return this;
			}
			public PostUpdateBuilder SetRole(Et_role? role)
			{
				base.SetField("role", role, 4);
				return this;
			}
			public PostUpdateBuilder SetText(JToken text)
			{
				base.SetField("text", NpgsqlDbType.Json, text, -1);
				return this;
			}
		}

	}
}
