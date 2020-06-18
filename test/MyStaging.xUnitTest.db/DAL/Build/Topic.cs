using MyStaging.Helpers;
using MyStaging.PostgreSQL;
using MyStaging.xUnitTest.Model;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyStaging.xUnitTest.DAL
{
    public partial class Topic : PgDbContext<TopicModel>
    {
        public static Topic Context { get { return new Topic(); } }

        public static InsertBuilder<TopicModel> InsertBuilder => new InsertBuilder<TopicModel>();
        public static TopicModel Insert(TopicModel model) => InsertBuilder.Insert(model);
        public static int InsertRange(List<TopicModel> models) => InsertBuilder.InsertRange(models).SaveChange();

        public static DeleteBuilder<TopicModel> DeleteBuilder => new DeleteBuilder<TopicModel>();
        public static int Delete(Guid id)
        {
            var affrows = DeleteBuilder.Where(f => f.Id == id).SaveChange();
            if (affrows > 0) ContextManager.CacheManager?.RemoveItemCache<TopicModel>(id.ToString());
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
                base.SetField("id", id);
                return this;
            }
            public TopicUpdateBuilder SetTitle(string title)
            {
                base.SetField("title", title);
                return this;
            }
            public TopicUpdateBuilder SetCreate_time(DateTime? create_time)
            {
                base.SetField("create_time", create_time);
                return this;
            }
            public TopicUpdateBuilder SetUpdate_time(DateTime? update_time)
            {
                base.SetField("update_time", update_time);
                return this;
            }
            public TopicUpdateBuilder SetLast_time(DateTime? last_time)
            {
                base.SetField("last_time", last_time);
                return this;
            }
            public TopicUpdateBuilder SetUser_id(Guid? user_id)
            {
                base.SetField("user_id", user_id);
                return this;
            }
            public TopicUpdateBuilder SetName(string name)
            {
                base.SetField("name", name);
                return this;
            }
            public TopicUpdateBuilder SetAge(int? age)
            {
                base.SetField("age", age);
                return this;
            }
            public TopicUpdateBuilder SetSex(bool? sex)
            {
                base.SetField("sex", sex);
                return this;
            }
            public TopicUpdateBuilder SetCreatetime(DateTime? createtime)
            {
                base.SetField("createtime", createtime);
                return this;
            }
            public TopicUpdateBuilder SetUpdatetime(TimeSpan? updatetime)
            {
                base.SetField("updatetime", updatetime);
                return this;
            }
        }

    }
}
