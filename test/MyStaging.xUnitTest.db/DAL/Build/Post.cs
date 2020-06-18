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
	public partial class Post : PgDbContext<PostModel>
	{
		public static Post Context { get { return new Post(); } }

		public static InsertBuilder<PostModel> InsertBuilder => new InsertBuilder<PostModel>();
		public static PostModel Insert(PostModel model) => InsertBuilder.Insert(model);
		public static int InsertRange(List<PostModel> models) => InsertBuilder.InsertRange(models).SaveChange();

		public static DeleteBuilder<PostModel> DeleteBuilder => new DeleteBuilder<PostModel>();
		public static int Delete(Guid id)
		{
			var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
			if (affrows > 0) ContextManager.CacheManager?.RemoveItemCache<PostModel>(id.ToString());
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
				base.SetField("id", id);
				return this;
			}
			public PostUpdateBuilder SetTitle(string title)
			{
				base.SetField("title", title);
				return this;
			}
			public PostUpdateBuilder SetContent(JToken content)
			{
				base.SetField("content", content);
				return this;
			}
			public PostUpdateBuilder SetState(Et_data_state? state)
			{
				base.SetField("state", state);
				return this;
			}
			public PostUpdateBuilder SetRole(Et_role? role)
			{
				base.SetField("role", role);
				return this;
			}
			public PostUpdateBuilder SetText(JToken text)
			{
				base.SetField("text", text);
				return this;
			}
		}

	}
}
