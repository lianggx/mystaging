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
	public partial class Topic : QueryContext<TopicModel>
	{
		public static Topic Context { get { return new Topic(); } }

		public static InsertBuilder<TopicModel> InsertBuilder => new InsertBuilder<TopicModel>(TopicSchema.Instance);
		public static TopicModel Insert(TopicModel model) => InsertBuilder.Insert(model);
		public static int InsertRange(List<TopicModel> models) => InsertBuilder.InsertRange(models).SaveChange();

		public static DeleteBuilder<TopicModel> DeleteBuilder => new DeleteBuilder<TopicModel>();
		public static int Delete(Guid id)
		{
			var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
			if (affrows > 0) PgSqlHelper.CacheManager?.RemoveItemCache<TopicModel>(id.ToString());
			return affrows;
			}

		public static TopicUpdateBuilder UpdateBuilder => new TopicUpdateBuilder();
		public static TopicUpdateBuilder Update(Guid id)
		{
			return new TopicUpdateBuilder(null, id);
		}

		public class TopicUpdateBuilder : UpdateBuilder<TopicModel>
		{
			public TopicUpdateBuilder(Guid id)
			{
				base.Where(f => f.Id == id);
			}

			public TopicUpdateBuilder(Action<TopicModel> onChanged, Guid id) : base(onChanged)
			{
				base.Where(f => f.Id == id);
			}

			public TopicUpdateBuilder() { }

			public new TopicUpdateBuilder Where(Expression<Func<TopicModel, bool>> predicate)
			{
				base.Where(predicate);
				return this;
			}
			public new TopicUpdateBuilder Where(string expression)
			{
				base.Where(expression);
				return this;
			}
			public TopicUpdateBuilder SetId(Guid id)
			{
				base.SetField("id", NpgsqlDbType.Uuid, id, 16);
				return this;
			}
			public TopicUpdateBuilder SetTitle(string title)
			{
				base.SetField("title", NpgsqlDbType.Varchar, title, 255);
				return this;
			}
			public TopicUpdateBuilder SetCreate_time(DateTime? create_time)
			{
				base.SetField("create_time", NpgsqlDbType.Timestamp, create_time, 8);
				return this;
			}
			public TopicUpdateBuilder SetUpdate_time(DateTime? update_time)
			{
				base.SetField("update_time", NpgsqlDbType.Timestamp, update_time, 8);
				return this;
			}
			public TopicUpdateBuilder SetLast_time(DateTime? last_time)
			{
				base.SetField("last_time", NpgsqlDbType.Timestamp, last_time, 8);
				return this;
			}
			public TopicUpdateBuilder SetUser_id(Guid? user_id)
			{
				base.SetField("user_id", NpgsqlDbType.Uuid, user_id, 16);
				return this;
			}
			public TopicUpdateBuilder SetName(string name)
			{
				base.SetField("name", NpgsqlDbType.Varchar, name, 255);
				return this;
			}
			public TopicUpdateBuilder SetAge(int? age)
			{
				base.SetField("age", NpgsqlDbType.Integer, age, 4);
				return this;
			}
			public TopicUpdateBuilder SetSex(bool? sex)
			{
				base.SetField("sex", NpgsqlDbType.Boolean, sex, 1);
				return this;
			}
			public TopicUpdateBuilder SetCreatetime(DateTime? createtime)
			{
				base.SetField("createtime", NpgsqlDbType.Date, createtime, 4);
				return this;
			}
			public TopicUpdateBuilder SetUpdatetime(TimeSpan? updatetime)
			{
				base.SetField("updatetime", NpgsqlDbType.Time, updatetime, 8);
				return this;
			}
		}

	}
}
